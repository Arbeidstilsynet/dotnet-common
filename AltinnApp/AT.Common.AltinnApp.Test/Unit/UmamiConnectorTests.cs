using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Channels;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class UmamiConnectorTests
{
    private const string PocWebsiteId = "86065263-1955-4c5e-8fa9-8ff7ddb1b349";
    private const string TestCollectorOrigin = "https://umami.example.com";

    [Theory]
    [InlineData("Development")]
    [InlineData("")]
    public void ResolveConfig_WhenEnvironmentIsDevelopmentOrMissing_ReturnsNull(
        string environmentName
    )
    {
        var config = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["Umami:WebsiteId"] = PocWebsiteId,
                ["Umami:AppPath"] = "/dat/forhandsmelding",
            }
        );

        var result = UmamiConnector.ResolveConfig(new FakeHostEnvironment(environmentName), config);

        result.ShouldBeNull();
    }

    [Fact]
    public void ResolveConfig_WhenEnableInDevelopmentIsTrue_EnablesTrackingWithLocalDefaults()
    {
        var config = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["Umami:WebsiteId"] = PocWebsiteId,
                ["Umami:EnableInDevelopment"] = "true",
                ["Umami:CollectorOrigin"] = "http://localhost:3003",
                ["Umami:ScriptPath"] = "/public/tracker.js",
                ["Umami:AppPath"] = "/dat/forhandsmelding",
            }
        );

        var result = UmamiConnector.ResolveConfig(
            new FakeHostEnvironment(Environments.Development),
            config
        );

        result.ShouldNotBeNull();
        result.WebsiteId.ShouldBe(PocWebsiteId);
        result.ScriptUrl.ShouldBe("http://localhost:3003/public/tracker.js");
        result.SendUrl.ShouldBe(new Uri("http://localhost:3003/public/api/send"));
        result.HostUrl.ShouldBe("http://localhost:3003/public");
        result.AppOrigin.ShouldBe(new Uri("http://local.altinn.cloud:8000"));
        result.AppHostname.ShouldBe("local.altinn.cloud");
        result.AppPath.ShouldBe("/dat/forhandsmelding");
    }

    [Theory]
    [InlineData("script.js")]
    [InlineData("//evil.example/script.js")]
    [InlineData("/public/../script.js")]
    [InlineData("/public/./tracker.js")]
    [InlineData("/tracker.js?v=1")]
    [InlineData("/tracker.js#hash")]
    [InlineData("/public/")]
    [InlineData("https://evil.example/script.js")]
    public void ResolveConfig_WhenScriptPathIsInvalid_ThrowsInvalidOperationException(
        string scriptPath
    )
    {
        var config = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["Umami:WebsiteId"] = PocWebsiteId,
                ["Umami:CollectorOrigin"] = TestCollectorOrigin,
                ["Umami:ScriptPath"] = scriptPath,
                ["Umami:AppPath"] = "/dat/forhandsmelding",
            }
        );

        Should
            .Throw<InvalidOperationException>(() =>
                UmamiConnector.ResolveConfig(new FakeHostEnvironment(Environments.Staging), config)
            )
            .Message.ShouldContain("Umami:ScriptPath");
    }

    [Fact]
    public void UmamiConnector_WhenConfigurationIsInvalid_DoesNotThrowAndRendersEmptyScriptTag()
    {
        var config = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["Umami:WebsiteId"] = "not-a-valid-uuid",
                ["Umami:CollectorOrigin"] = "not-a-valid-origin",
            }
        );

        var connector = new UmamiConnector(new FakeHostEnvironment(Environments.Staging), config);

        connector.Config.ShouldBeNull();
        using var writer = new StringWriter();
        connector.RenderScriptTag().WriteTo(writer, HtmlEncoder.Default);
        writer.ToString().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveConfig_WhenWebsiteIdIsMissing_ReturnsNull(string? websiteId)
    {
        var config = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["Umami:WebsiteId"] = websiteId,
                ["Umami:AppPath"] = "/dat/forhandsmelding",
            }
        );

        var result = UmamiConnector.ResolveConfig(
            new FakeHostEnvironment(Environments.Staging),
            config
        );

        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("placeholder")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("8606526319554c5e8fa98ff7ddb1b349")]
    public void ResolveConfig_WhenWebsiteIdIsInvalid_ThrowsInvalidOperationException(
        string websiteId
    )
    {
        var config = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["Umami:WebsiteId"] = websiteId,
                ["Umami:CollectorOrigin"] = TestCollectorOrigin,
                ["Umami:AppPath"] = "/dat/forhandsmelding",
            }
        );

        Should.Throw<InvalidOperationException>(() =>
            UmamiConnector.ResolveConfig(new FakeHostEnvironment(Environments.Staging), config)
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://umami.example.com")]
    [InlineData("https://umami.example.com/path")]
    [InlineData("https://umami.example.com?query=1")]
    [InlineData("https://umami.example.com#hash")]
    [InlineData("https://user:pass@umami.example.com")]
    public void ResolveConfig_WhenCollectorOriginIsMissingOrInvalid_ThrowsInvalidOperationException(
        string? collectorOrigin
    )
    {
        var config = CreateConfiguration(
            new Dictionary<string, string?>
            {
                ["Umami:WebsiteId"] = PocWebsiteId,
                ["Umami:CollectorOrigin"] = collectorOrigin,
                ["Umami:AppPath"] = "/dat/forhandsmelding",
            }
        );

        Should.Throw<InvalidOperationException>(() =>
            UmamiConnector.ResolveConfig(new FakeHostEnvironment(Environments.Staging), config)
        );
    }

    [Fact]
    public void ResolveConfig_AutoDiscoversAppPathFromAppDirectory()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"altinn-umami-test-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(Path.Combine(tempRoot, "config"));
            File.WriteAllText(
                Path.Combine(tempRoot, "config", "applicationmetadata.json"),
                """{"id":"dat/forhandsmelding"}"""
            );

            var config = CreateConfiguration(
                new Dictionary<string, string?>
                {
                    ["Umami:WebsiteId"] = PocWebsiteId.ToUpperInvariant(),
                    ["Umami:CollectorOrigin"] = TestCollectorOrigin,
                }
            );

            var result = UmamiConnector.ResolveConfig(
                new FakeHostEnvironment(Environments.Staging, tempRoot),
                config
            );

            result.ShouldNotBeNull();
            result.WebsiteId.ShouldBe(PocWebsiteId);
            result.ScriptUrl.ShouldBe("https://umami.example.com/script.js");
            result.AppHostname.ShouldBe("dat.apps.tt02.altinn.no");
            result.AppPath.ShouldBe("/dat/forhandsmelding");
            result.AppTitle.ShouldBe("Forhandsmelding");
            result.SendUrl.ShouldBe(new Uri("https://umami.example.com/api/send"));
            result.HostUrl.ShouldBe("https://umami.example.com");
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void AddUmamiTracking_RegistersConnectorAndRendersUmamiScriptTag()
    {
        using var provider = CreateTrackingServices().BuildServiceProvider();
        var connector = provider.GetRequiredService<IUmamiConnector>();

        using var writer = new StringWriter();
        connector.RenderScriptTag().WriteTo(writer, HtmlEncoder.Default);

        var html = writer.ToString();
        html.ShouldBe(
            $"<script>window.umamiConfig={{websiteId:\"{PocWebsiteId}\",beforeSend:{UmamiConnector.BeforeSendFunction("/dat/forhandsmelding")}}};</script>"
                + "<script defer src=\"https://umami.example.com/public/tracker.js\" data-host-url=\"https://umami.example.com/public\" "
                + "data-domains=\"dat.apps.tt02.altinn.no\" data-exclude-search=\"true\" "
                + "data-do-not-track=\"true\" data-errors=\"true\"></script>"
        );
        // The website ID must only be set together with the URL sanitizer, so a blocked inline script disables tracking.
        html.ShouldNotContain("data-website-id");
        html.ShouldNotContain("data-exclude-hash");
    }

    [Fact]
    public void RenderScriptTag_WithNonce_AddsEncodedNonceToBothScripts()
    {
        using var provider = CreateTrackingServices().BuildServiceProvider();
        var connector = provider.GetRequiredService<IUmamiConnector>();

        using var writer = new StringWriter();
        connector.RenderScriptTag("abc\"123").WriteTo(writer, HtmlEncoder.Default);

        var html = writer.ToString();
        html.ShouldStartWith("<script nonce=\"abc&quot;123\">window.umamiConfig=");
        html.ShouldContain("</script><script nonce=\"abc&quot;123\" defer src=");
    }

    private const string InstanceRoute =
        "https://dat.apps.altinn.no/dat/forhandsmelding/#/instance/500002/f1111111-2222-3333-4444-555555555555";

    [Theory]
    [InlineData(InstanceRoute + "/Task_1/Side1?x=1", "/dat/forhandsmelding/Task_1/Side1")]
    [InlineData(InstanceRoute + "/Task_1", "/dat/forhandsmelding/Task_1")]
    [InlineData(
        InstanceRoute + "/Task_1/Side1/subform/f2222222-2222-3333-4444-555555555555/Underside",
        "/dat/forhandsmelding/Task_1/Side1"
    )]
    [InlineData(InstanceRoute + "/Task_1/S%C3%B8knad", "/dat/forhandsmelding/Task_1/Søknad")]
    [InlineData(InstanceRoute + "/Task_1/Side%3Cx", "/dat/forhandsmelding/Task_1")]
    [InlineData("https://dat.apps.altinn.no/dat/forhandsmelding/", "/dat/forhandsmelding")]
    [InlineData(
        "https://dat.apps.altinn.no/dat/forhandsmelding/#/Side1",
        "/dat/forhandsmelding/Side1"
    )]
    [InlineData(
        "https://dat.apps.altinn.no/dat/forhandsmelding/#/Side2?x=1",
        "/dat/forhandsmelding/Side2"
    )]
    [InlineData(
        "https://dat.apps.altinn.no/dat/forhandsmelding/#/party-selection/403",
        "/dat/forhandsmelding/party-selection"
    )]
    [InlineData(
        "https://dat.apps.altinn.no/dat/forhandsmelding/#/instance-selection",
        "/dat/forhandsmelding/instance-selection"
    )]
    [InlineData(InstanceRoute, "/dat/forhandsmelding")]
    [InlineData(InstanceRoute + "/Task_1/%E0%A4%A", null)]
    public void BeforeSendFunction_ReportsOnlyAppPathTaskAndPage(string href, string? expected)
    {
        var result = RunBeforeSend(href, "event", new { url = href, referrer = "" });

        if (expected is null)
        {
            result.ValueKind.ShouldBe(JsonValueKind.Null);
            return;
        }

        result.GetProperty("url").GetString().ShouldBe(expected);
    }

    [Fact]
    public void BeforeSendFunction_DropsSameOriginReferrersAndSetsErrorUrl()
    {
        var internalReferrer = RunBeforeSend(
            InstanceRoute + "/Task_1/Side2",
            "event",
            new
            {
                url = "",
                referrer = "/dat/forhandsmelding/#/instance/500002/f1111111-2222-3333-4444-555555555555/Task_1/Side1",
            }
        );
        var externalReferrer = RunBeforeSend(
            InstanceRoute + "/Task_1/Side1",
            "event",
            new { url = "", referrer = "https://www.altinn.no/" }
        );
        var error = RunBeforeSend(
            InstanceRoute + "/Task_1/Side1",
            "error",
            new { url = "/dat/forhandsmelding/" }
        );

        internalReferrer.GetProperty("referrer").GetString().ShouldBeEmpty();
        externalReferrer.GetProperty("referrer").GetString().ShouldBe("https://www.altinn.no/");
        error.GetProperty("url").GetString().ShouldBe("/dat/forhandsmelding/Task_1/Side1");
    }

    private static JsonElement RunBeforeSend(string href, string type, object payload)
    {
        var script =
            $"globalThis.location=new URL({JsonSerializer.Serialize(href)});"
            + $"const beforeSend=({UmamiConnector.BeforeSendFunction("/dat/forhandsmelding")});"
            + $"process.stdout.write(JSON.stringify(beforeSend({JsonSerializer.Serialize(type)},{JsonSerializer.Serialize(payload)})??null));";

        System.Diagnostics.Process process;
        try
        {
            process = System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo("node", "-")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            )!;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            Assert.Skip("Node.js is required to run the Umami beforeSend sanitizer tests.");
            throw;
        }

        using (process)
        {
            process.StandardInput.Write(script);
            process.StandardInput.Close();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.ExitCode.ShouldBe(0, error);
            return JsonDocument.Parse(output).RootElement.Clone();
        }
    }

    [Fact]
    public void UmamiErrorLoggerProvider_IgnoresPlatformHttpExceptionsByDefault()
    {
        var queue = new UmamiErrorQueue();
        using var loggerProvider = new UmamiErrorLoggerProvider(queue);
        var logger = loggerProvider.CreateLogger("Altinn.App.Api.Controllers.ProcessController");
        var platformException = new Altinn.App.Core.Helpers.PlatformHttpException(
            new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway),
            "Storage unavailable"
        );

        logger.LogError(platformException, "Direct");
        logger.LogError(new InvalidOperationException("Wrapped", platformException), "Wrapped");
        logger.LogError(new AggregateException(new TimeoutException(), platformException), "Agg");
        logger.LogError(new TimeoutException(), "Reported");

        queue.TryDequeue(out var errorEvent).ShouldBeTrue();
        errorEvent.ErrorType.ShouldBe(nameof(TimeoutException));
        queue.TryDequeue(out _).ShouldBeFalse();
    }

    [Fact]
    public void UmamiErrorLoggerProvider_UsesConfiguredIgnoreError()
    {
        var queue = new UmamiErrorQueue();
        using var loggerProvider = new UmamiErrorLoggerProvider(
            queue,
            options: new UmamiConfiguration { IgnoreError = ex => ex is TimeoutException }
        );
        var logger = loggerProvider.CreateLogger("Altinn.App.Api.Controllers.ProcessController");

        logger.LogError(new TimeoutException(), "Ignored");
        logger.LogError(
            new Altinn.App.Core.Helpers.PlatformHttpException(
                new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway),
                "Reported when the default is replaced"
            ),
            "Reported"
        );

        queue.TryDequeue(out var errorEvent).ShouldBeTrue();
        errorEvent.ErrorType.ShouldBe("PlatformHttpException");
        queue.TryDequeue(out _).ShouldBeFalse();
    }

    [Fact]
    public void UmamiErrorLoggerProvider_QueuesOnlyErrorsWithExceptionsAndOnlyOncePerException()
    {
        var queue = new UmamiErrorQueue();
        var instanceGuid = Guid.Parse("f1111111-2222-3333-4444-555555555555");
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["instanceOwnerPartyId"] = 500002;
        httpContext.Request.RouteValues["instanceGuid"] = instanceGuid;
        using var loggerProvider = new UmamiErrorLoggerProvider(
            queue,
            new HttpContextAccessor { HttpContext = httpContext }
        );
        var logger = loggerProvider.CreateLogger("Altinn.App.Api.Controllers.ProcessController");
        var exception = Should.Throw<InvalidOperationException>(ThrowFromAsyncMethod);

        logger.LogError(exception, "Process next failed.");
        logger.LogCritical(exception, "Logged again further up the stack.");
        logger.LogWarning(new TimeoutException(), "Not an error.");
        logger.LogError("Error without exception.");

        queue.TryDequeue(out var errorEvent).ShouldBeTrue();
        errorEvent.ErrorType.ShouldBe(nameof(InvalidOperationException));
        errorEvent.ErrorLocation.ShouldNotBeNull();
        errorEvent.ErrorLocation.ShouldStartWith(
            $"{nameof(UmamiConnectorTests)}.{nameof(ThrowFromAsyncMethod)}"
        );
        errorEvent.InstanceId.ShouldBe(instanceGuid.ToString());
        queue.TryDequeue(out _).ShouldBeFalse();
    }

    [Fact]
    public void UmamiErrorLoggerProvider_ThrottlesRepeatedErrorsFromTheSameLocation()
    {
        var queue = new UmamiErrorQueue();
        var time = new ManualTimeProvider();
        using var loggerProvider = new UmamiErrorLoggerProvider(queue, timeProvider: time);
        var logger = loggerProvider.CreateLogger("Altinn.App.Api.Controllers.ProcessController");

        logger.LogError(Should.Throw<InvalidOperationException>(ThrowFromAsyncMethod), "First");
        time.Advance(TimeSpan.FromSeconds(2));
        logger.LogError(Should.Throw<InvalidOperationException>(ThrowFromAsyncMethod), "Retry");
        logger.LogError(new TimeoutException(), "Different error");
        time.Advance(UmamiErrorLoggerProvider.ThrottleWindow);
        logger.LogError(Should.Throw<InvalidOperationException>(ThrowFromAsyncMethod), "Later");

        var reported = new List<string>();
        while (queue.TryDequeue(out var errorEvent))
        {
            reported.Add(errorEvent.ErrorType);
        }
        reported.ShouldBe([
            nameof(InvalidOperationException),
            nameof(TimeoutException),
            nameof(InvalidOperationException),
        ]);
    }

    [Fact]
    public void UmamiErrorLocation_UnwrapsLambdasToDeclaringMember()
    {
        Action throwFromLambda = () => throw new InvalidOperationException("Simulated failure");
        var exception = Should.Throw<InvalidOperationException>(throwFromLambda);

        UmamiErrorLocation
            .Resolve(exception)
            .ShouldStartWith(
                $"{nameof(UmamiConnectorTests)}.{nameof(UmamiErrorLocation_UnwrapsLambdasToDeclaringMember)}"
            );
    }

    [Fact]
    public async Task AddUmamiTracking_WhenErrorIsLogged_SendsAltinnErrorEventToUmami()
    {
        var handler = new CapturingHandler();
        var services = CreateTrackingServices();
        services
            .AddHttpClient(UmamiErrorReporter.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        await using var provider = services.BuildServiceProvider();
        var reporter = provider.GetServices<IHostedService>().OfType<UmamiErrorReporter>().Single();
        await reporter.StartAsync(CancellationToken.None);

        var exception = Should.Throw<InvalidOperationException>(ThrowFromAsyncMethod);
        provider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Altinn.App.Api.Controllers.ProcessController")
            .LogError(exception, "Process next failed.");

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var (request, body) = await handler.Requests.Reader.ReadAsync(timeout.Token);
        await reporter.StopAsync(CancellationToken.None);

        request.Method.ShouldBe(HttpMethod.Post);
        request.RequestUri.ShouldBe(new Uri("https://umami.example.com/public/api/send"));
        request
            .Headers.UserAgent.ToString()
            .ShouldStartWith("Mozilla/5.0 (compatible; Arbeidstilsynet.Common.AltinnApp/");
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("type").GetString().ShouldBe("event");
        var payload = json.RootElement.GetProperty("payload");
        payload.GetProperty("website").GetString().ShouldBe(PocWebsiteId);
        payload.GetProperty("hostname").GetString().ShouldBe("dat.apps.tt02.altinn.no");
        payload.GetProperty("url").GetString().ShouldBe("/dat/forhandsmelding");
        payload.GetProperty("title").GetString().ShouldBe("Forhandsmelding");
        payload.GetProperty("name").GetString().ShouldBe("altinn-error");
        var data = payload.GetProperty("data");
        data.EnumerateObject().Select(p => p.Name).ShouldBe(["errorType", "errorLocation"]);
        data.GetProperty("errorType").GetString().ShouldBe(nameof(InvalidOperationException));
        data.GetProperty("errorLocation")
            .GetString()
            .ShouldStartWith($"{nameof(UmamiConnectorTests)}.{nameof(ThrowFromAsyncMethod)}");
    }

    private static async Task ThrowFromAsyncMethod()
    {
        await Task.Yield();
        throw new InvalidOperationException("Simulated processor failure");
    }

    private static ServiceCollection CreateTrackingServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment(Environments.Staging));
        services.AddSingleton(
            CreateConfiguration(
                new Dictionary<string, string?> { ["Umami:WebsiteId"] = PocWebsiteId }
            )
        );
        services.AddUmamiTracking(options =>
            options with
            {
                CollectorOrigin = TestCollectorOrigin,
                ScriptPath = "/public/tracker.js",
                AppPath = "/dat/forhandsmelding",
                AppTitle = "Forhandsmelding",
            }
        );
        return services;
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UnixEpoch;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public Channel<(HttpRequestMessage Request, string Body)> Requests { get; } =
            Channel.CreateUnbounded<(HttpRequestMessage, string)>();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            await Requests.Writer.WriteAsync((request, body), cancellationToken);
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
    }

    private static IConfiguration CreateConfiguration(IDictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private sealed class FakeHostEnvironment(string environmentName, string? contentRootPath = null)
        : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Altinn.Application";
        public string ContentRootPath { get; set; } = contentRootPath ?? AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
