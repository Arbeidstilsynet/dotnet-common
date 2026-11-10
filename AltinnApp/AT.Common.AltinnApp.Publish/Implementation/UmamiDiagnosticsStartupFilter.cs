using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal sealed class UmamiDiagnosticsStartupFilter : IStartupFilter
{
    public const string TraceIdHeaderName = "x-trace-id";
    public const string ErrorTypeHeaderName = "x-error-type";
    public const string ErrorLocationHeaderName = "x-error-location";
    public const string EnrichmentsHeaderName = "x-umami-props";
    public const string EventsHeaderName = "x-umami-events";

    private const int MaxHeaderByteLength = 4096;

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(
                async (context, pipelineNext) =>
                {
                    SetTraceIdHeader(context);
                    context.Response.OnStarting(() => WriteEnrichmentHeadersAsync(context, null));
                    try
                    {
                        await pipelineNext(context);
                        await WriteEnrichmentHeadersAsync(context, null);
                    }
                    catch (Exception ex)
                    {
                        await SetErrorHeadersAsync(context, ex);
                        throw;
                    }
                }
            );
            next(app);
        };
    }

    private static void SetTraceIdHeader(HttpContext context)
    {
        var traceId = ResolveTraceId(context);
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            context.Response.Headers[TraceIdHeaderName] = traceId;
        }
    }

    private static async Task SetErrorHeadersAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        SetTraceIdHeader(context);
        context.Response.Headers[ErrorTypeHeaderName] = exception.GetType().Name;
        var location = ResolveErrorLocation(exception);
        if (!string.IsNullOrWhiteSpace(location))
        {
            context.Response.Headers[ErrorLocationHeaderName] = location;
        }

        await WriteEnrichmentHeadersAsync(context, exception);
    }

    private static async Task WriteEnrichmentHeadersAsync(HttpContext context, Exception? exception)
    {
        if (context.Response.HasStarted || context.RequestServices is null)
        {
            return;
        }

        try
        {
            var recorder = context.RequestServices.GetService<UmamiEventRecorder>();
            if (recorder is null)
            {
                return;
            }

            var enrichers = context.RequestServices.GetServices<IUmamiEventEnricher>();
            foreach (var enricher in enrichers)
            {
                try
                {
                    await enricher.EnrichAsync(context, recorder, exception);
                }
                catch
                {
                    // Never allow custom enrichers to fail the Altinn request pipeline.
                }
            }

            if (recorder.Enrichments.Count > 0)
            {
                var propsJson = JsonSerializer.Serialize(recorder.Enrichments);
                if (propsJson.Length <= MaxHeaderByteLength)
                {
                    context.Response.Headers[EnrichmentsHeaderName] = propsJson;
                }
            }

            if (recorder.Events.Count > 0)
            {
                var eventsJson = JsonSerializer.Serialize(recorder.Events);
                if (eventsJson.Length <= MaxHeaderByteLength)
                {
                    context.Response.Headers[EventsHeaderName] = eventsJson;
                }
            }
        }
        catch
        {
            // Never allow analytics header serialization to fail the request.
        }
    }

    internal static string? ResolveErrorLocation(Exception exception)
    {
        var trace = new StackTrace(exception, fNeedFileInfo: true);
        var frames = trace.GetFrames();
        if (frames is not { Length: > 0 })
        {
            return null;
        }

        var selectedFrame =
            frames.FirstOrDefault(IsApplicationFrame)
            ?? frames.FirstOrDefault(frame => frame.GetFileLineNumber() > 0)
            ?? frames[0];

        return FormatStackFrame(selectedFrame);
    }

    private static bool IsApplicationFrame(StackFrame frame)
    {
        var ns = frame.GetMethod()?.DeclaringType?.Namespace;
        if (string.IsNullOrWhiteSpace(ns))
        {
            return frame.GetFileLineNumber() > 0;
        }

        return !ns.StartsWith("System", StringComparison.Ordinal)
            && !ns.StartsWith("Microsoft", StringComparison.Ordinal);
    }

    private static string? FormatStackFrame(StackFrame frame)
    {
        var method = frame.GetMethod();
        var typeName = FormatDeclaringTypeName(method?.DeclaringType);
        var methodName = method?.Name;
        var fileName = Path.GetFileName(frame.GetFileName());
        var lineNumber = frame.GetFileLineNumber();

        var symbol =
            !string.IsNullOrWhiteSpace(typeName) && !string.IsNullOrWhiteSpace(methodName)
                ? $"{typeName}.{methodName}"
                : methodName ?? typeName;

        if (!string.IsNullOrWhiteSpace(fileName) && lineNumber > 0)
        {
            return string.IsNullOrWhiteSpace(symbol)
                ? $"{fileName}:{lineNumber}"
                : $"{symbol} ({fileName}:{lineNumber})";
        }

        return symbol;
    }

    private static string? FormatDeclaringTypeName(Type? declaringType)
    {
        if (declaringType is null)
        {
            return null;
        }

        if (declaringType.DeclaringType is { } outerType && declaringType.Name.StartsWith('<'))
        {
            return outerType.Name;
        }

        return declaringType.Name;
    }

    private static string? ResolveTraceId(HttpContext context)
    {
        if (Activity.Current is { } activity)
        {
            var activityTraceId = activity.TraceId.ToHexString();
            if (
                !string.IsNullOrWhiteSpace(activityTraceId)
                && activityTraceId != "00000000000000000000000000000000"
            )
            {
                return activityTraceId;
            }

            if (!string.IsNullOrWhiteSpace(activity.Id))
            {
                return activity.Id;
            }
        }

        return context.TraceIdentifier;
    }
}
