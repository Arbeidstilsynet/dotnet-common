using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Channels;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Ports;
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

        writer
            .ToString()
            .ShouldBe(
                $"<script defer src=\"https://umami.example.com/public/tracker.js\" data-website-id=\"{PocWebsiteId}\" "
                    + "data-domains=\"dat.apps.tt02.altinn.no\" data-exclude-search=\"true\" data-exclude-hash=\"true\" "
                    + "data-do-not-track=\"true\"></script>"
            );
    }

    [Fact]
    public void UmamiErrorLoggerProvider_QueuesOnlyErrorsWithExceptionsAndOnlyOncePerException()
    {
        var queue = new UmamiErrorQueue();
        using var loggerProvider = new UmamiErrorLoggerProvider(queue);
        var logger = loggerProvider.CreateLogger("Altinn.App.Api.Controllers.ProcessController");
        using var activity = new Activity("test-request").Start();
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
        errorEvent.TraceId.ShouldBe(activity.TraceId.ToHexString());
        queue.TryDequeue(out _).ShouldBeFalse();
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
