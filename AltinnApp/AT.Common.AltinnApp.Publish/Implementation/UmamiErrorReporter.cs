using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal sealed record UmamiErrorEvent(string ErrorType, string? ErrorLocation, string? TraceId);

internal sealed class UmamiErrorQueue
{
    private readonly Channel<UmamiErrorEvent> _channel = Channel.CreateBounded<UmamiErrorEvent>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
        }
    );

    public bool TryEnqueue(UmamiErrorEvent errorEvent) => _channel.Writer.TryWrite(errorEvent);

    public bool TryDequeue(out UmamiErrorEvent errorEvent) =>
        _channel.Reader.TryRead(out errorEvent!);

    public void Complete() => _channel.Writer.TryComplete();

    public IAsyncEnumerable<UmamiErrorEvent> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}

/// <summary>
/// Captures exceptions logged at <see cref="LogLevel.Error"/> or higher, including exceptions that Altinn controllers
/// catch and convert to HTTP 500 responses, and queues them as Umami error events.
/// </summary>
internal sealed class UmamiErrorLoggerProvider(UmamiErrorQueue queue) : ILoggerProvider
{
    private static readonly object Reported = new();
    private readonly ConditionalWeakTable<Exception, object> _reportedExceptions = new();

    public ILogger CreateLogger(string categoryName) =>
        IsOwnCategory(categoryName) ? NullLogger.Instance : new UmamiErrorLogger(this);

    public void Dispose() { }

    internal void Report(Exception exception)
    {
        // The same exception is often logged more than once as it propagates.
        if (!_reportedExceptions.TryAdd(exception, Reported))
        {
            return;
        }

        var traceId = Activity.Current?.TraceId;
        queue.TryEnqueue(
            new UmamiErrorEvent(
                exception.GetType().Name,
                UmamiErrorLocation.Resolve(exception),
                traceId is { } id && id != default ? id.ToHexString() : null
            )
        );
    }

    private static bool IsOwnCategory(string categoryName) =>
        categoryName == typeof(UmamiErrorReporter).FullName
        || categoryName.StartsWith(
            $"System.Net.Http.HttpClient.{UmamiErrorReporter.HttpClientName}.",
            StringComparison.Ordinal
        );

    private sealed class UmamiErrorLogger(UmamiErrorLoggerProvider provider) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel is >= LogLevel.Error and < LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            if (exception is null || !IsEnabled(logLevel))
            {
                return;
            }

            try
            {
                provider.Report(exception);
            }
            catch
            {
                // Error reporting must never break application logging.
            }
        }
    }
}

/// <summary>
/// Sends queued error events to the Umami collection endpoint as <c>altinn-error</c> custom events.
/// </summary>
internal sealed class UmamiErrorReporter(
    UmamiErrorQueue queue,
    IUmamiConnector connector,
    IHttpClientFactory httpClientFactory,
    ILogger<UmamiErrorReporter> logger
) : BackgroundService
{
    public const string HttpClientName = "Umami";
    public const string ErrorEventName = "altinn-error";

    // Umami drops requests with bot-like User-Agents, such as a bare "Product/1.0" token.
    public static readonly string UserAgent =
        $"Mozilla/5.0 (compatible; Arbeidstilsynet.Common.AltinnApp/{typeof(UmamiErrorReporter).Assembly.GetName().Version?.ToString(3)})";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (connector.Config is not { } config)
        {
            queue.Complete();
            while (queue.TryDequeue(out _)) { }
            return;
        }

        try
        {
            await foreach (var errorEvent in queue.ReadAllAsync(stoppingToken))
            {
                await SendAsync(config, errorEvent, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is shutting down.
        }
    }

    internal async Task SendAsync(
        UmamiTrackingConfig config,
        UmamiErrorEvent errorEvent,
        CancellationToken cancellationToken
    )
    {
        var data = new Dictionary<string, string> { ["errorType"] = errorEvent.ErrorType };
        if (errorEvent.ErrorLocation is { } errorLocation)
        {
            data["errorLocation"] = errorLocation;
        }
        if (errorEvent.TraceId is { } traceId)
        {
            data["traceId"] = traceId;
        }

        var body = new
        {
            type = "event",
            payload = new
            {
                website = config.WebsiteId,
                hostname = config.AppHostname,
                url = config.AppPath,
                title = config.AppTitle,
                name = ErrorEventName,
                data,
            },
        };

        try
        {
            using var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.PostAsJsonAsync(
                config.SendUrl,
                body,
                cancellationToken
            );
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Umami rejected error event with status code {StatusCode}",
                    (int)response.StatusCode
                );
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Failed to send error event to Umami");
        }
    }
}

internal static partial class UmamiErrorLocation
{
    private const int MaxLength = 256;

    /// <summary>
    /// Resolves a <c>Type.Method (File.cs:line)</c> location for the most relevant stack frame, preferring frames in
    /// the application's entry assembly. File and line are only included when PDB symbols are available.
    /// </summary>
    public static string? Resolve(Exception exception)
    {
        var frames = new StackTrace(exception, fNeedFileInfo: true)
            .GetFrames()
            .Where(frame => frame.GetMethod() is not null)
            .ToList();
        if (frames.Count == 0)
        {
            return null;
        }

        var entryAssembly = Assembly.GetEntryAssembly();
        var selectedFrame =
            frames.FirstOrDefault(frame =>
                entryAssembly is not null
                && frame.GetMethod()!.DeclaringType?.Assembly == entryAssembly
            )
            ?? frames.FirstOrDefault(IsNonFrameworkFrame)
            ?? frames[0];

        var location = Format(selectedFrame);
        return location.Length > MaxLength ? location[..MaxLength] : location;
    }

    private static bool IsNonFrameworkFrame(StackFrame frame)
    {
        var ns = frame.GetMethod()!.DeclaringType?.Namespace;
        return ns is not null
            && !ns.StartsWith("System", StringComparison.Ordinal)
            && !ns.StartsWith("Microsoft", StringComparison.Ordinal);
    }

    private static string Format(StackFrame frame)
    {
        var method = frame.GetMethod()!;
        var methodName = OriginalName(method.Name) ?? method.Name;
        var type = method.DeclaringType;

        // Unwrap compiler-generated async state machines, lambdas and local functions to the user-written member.
        while (type is { DeclaringType: not null } && type.Name.StartsWith('<'))
        {
            if (methodName == "MoveNext")
            {
                methodName = OriginalName(type.Name) ?? methodName;
            }
            type = type.DeclaringType;
        }

        var symbol = type is null ? methodName : $"{type.Name.Split('`')[0]}.{methodName}";
        var fileName = Path.GetFileName(frame.GetFileName());
        var lineNumber = frame.GetFileLineNumber();

        return !string.IsNullOrEmpty(fileName) && lineNumber > 0
            ? $"{symbol} ({fileName}:{lineNumber})"
            : symbol;
    }

    private static string? OriginalName(string compilerGeneratedName)
    {
        var match = CompilerGeneratedNamePattern().Match(compilerGeneratedName);
        return match.Success ? match.Groups[1].Value : null;
    }

    [GeneratedRegex("^<+([^<>]+)>")]
    private static partial Regex CompilerGeneratedNamePattern();
}
