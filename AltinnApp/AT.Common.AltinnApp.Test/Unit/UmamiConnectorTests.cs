using System.Text.Encodings.Web;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class UmamiConnectorTests
{
    private const string PocWebsiteId = "86065263-1955-4c5e-8fa9-8ff7ddb1b349";

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

        var result = UmamiConnector.ResolveConfig(
            new FakeHostEnvironment(environmentName),
            config
        );

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
                ["Umami:AppPath"] = "/dat/forhandsmelding",
            }
        );

        var result = UmamiConnector.ResolveConfig(
            new FakeHostEnvironment(Environments.Development),
            config
        );

        result.ShouldNotBeNull();
        result.WebsiteId.ShouldBe(PocWebsiteId);
        result.AltinnScriptUrl.ShouldBe("http://localhost:3003/altinn.js");
        result.AppOrigin.ShouldBe(new Uri("http://local.altinn.cloud:8000"));
        result.AppHostname.ShouldBe("local.altinn.cloud");
        result.AppPath.ShouldBe("/dat/forhandsmelding");
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

        var connector = new UmamiConnector(
            new FakeHostEnvironment(Environments.Staging),
            config
        );

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
                ["Umami:AppPath"] = "/dat/forhandsmelding",
            }
        );

        Should.Throw<InvalidOperationException>(() =>
            UmamiConnector.ResolveConfig(new FakeHostEnvironment(Environments.Staging), config)
        );
    }

    [Theory]
    [InlineData("http://meldinger-umami-collect.arbeidstilsynet.no")]
    [InlineData("https://meldinger-umami-collect.arbeidstilsynet.no/path")]
    [InlineData("https://meldinger-umami-collect.arbeidstilsynet.no?query=1")]
    [InlineData("https://meldinger-umami-collect.arbeidstilsynet.no#hash")]
    [InlineData("https://user:pass@meldinger-umami-collect.arbeidstilsynet.no")]
    public void ResolveConfig_WhenCollectorOriginIsInvalid_ThrowsInvalidOperationException(
        string collectorOrigin
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
    public void ResolveConfig_AutoDiscoversAppPathAndStepsFromAppDirectory()
    {
        var tempRoot = Path.Combine(
            Path.GetTempPath(),
            $"altinn-umami-test-{Guid.NewGuid():N}"
        );
        try
        {
            Directory.CreateDirectory(Path.Combine(tempRoot, "config"));
            Directory.CreateDirectory(Path.Combine(tempRoot, "ui", "form"));
            File.WriteAllText(
                Path.Combine(tempRoot, "config", "applicationmetadata.json"),
                """{"id":"dat/forhandsmelding"}"""
            );
            File.WriteAllText(
                Path.Combine(tempRoot, "ui", "form", "Settings.json"),
                """{"pages":{"order":["0-startside","5-Byggherre","8-bekreftelse"]}}"""
            );

            var config = CreateConfiguration(
                new Dictionary<string, string?>
                {
                    ["Umami:WebsiteId"] = PocWebsiteId.ToUpperInvariant(),
                }
            );

            var result = UmamiConnector.ResolveConfig(
                new FakeHostEnvironment(Environments.Staging, tempRoot),
                config
            );

            result.ShouldNotBeNull();
            result.WebsiteId.ShouldBe(PocWebsiteId);
            result.AltinnScriptUrl.ShouldBe(
                "https://meldinger-umami-collect.arbeidstilsynet.no/altinn.js"
            );
            result.ScriptUrl.ShouldBe(
                "https://meldinger-umami-collect.arbeidstilsynet.no/script.js"
            );
            result.AppHostname.ShouldBe("dat.apps.tt02.altinn.no");
            result.AppPath.ShouldBe("/dat/forhandsmelding");
            result.AppTitle.ShouldBe("Forhandsmelding");
            result.AllowedSteps.ShouldBe(["0-startside", "5-Byggherre", "8-bekreftelse"]);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Theory]
    [InlineData(
        "/dat/forhandsmelding",
        null,
        "/dat/forhandsmelding",
        "Forhandsmelding"
    )]
    [InlineData(
        "/dat/forhandsmelding/",
        "#/instance/500002/f1111111-2222-3333-4444-555555555555",
        "/dat/forhandsmelding",
        "Forhandsmelding"
    )]
    [InlineData(
        "/dat/forhandsmelding/instance/500002/f1111111-2222-3333-4444-555555555555/Task_1/5-Byggherre",
        null,
        "/dat/forhandsmelding/5-Byggherre",
        "Forhandsmelding - 5-Byggherre"
    )]
    [InlineData(
        "/dat/forhandsmelding/",
        "#/instance/500002/f1111111-2222-3333-4444-555555555555/Task_1/8-bekreftelse?foo=secret",
        "/dat/forhandsmelding/8-bekreftelse",
        "Forhandsmelding - 8-bekreftelse"
    )]
    public void ResolveSanitizedPage_WhenPathIsInsideApp_StripsInstanceIdAndMapsAllowedStep(
        string pathname,
        string? hash,
        string expectedUrl,
        string expectedTitle
    )
    {
        var config = UmamiConnector.ResolveConfig(
            new FakeHostEnvironment(Environments.Staging),
            CreateConfiguration(
                new Dictionary<string, string?> { ["Umami:WebsiteId"] = PocWebsiteId }
            ),
            new UmamiConfiguration
            {
                AppPath = "/dat/forhandsmelding",
                AppTitle = "Forhandsmelding",
                AllowedSteps = ["0-startside", "5-Byggherre", "8-bekreftelse"],
            }
        );

        config.ShouldNotBeNull();
        var page = config.ResolveSanitizedPage(pathname, hash);

        page.ShouldNotBeNull();
        page.Url.ShouldBe(expectedUrl);
        page.Title.ShouldBe(expectedTitle);
        page.Url.ShouldNotContain("500002");
        page.Url.ShouldNotContain("f1111111-2222-3333-4444-555555555555");
    }

    [Fact]
    public void AddUmamiTracking_RegistersConnectorAndRendersScriptTag()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment(Environments.Staging));
        services.AddSingleton(
            CreateConfiguration(
                new Dictionary<string, string?> { ["Umami:WebsiteId"] = PocWebsiteId }
            )
        );
        services.AddUmamiTracking(options =>
            options with
            {
                AppPath = "/dat/forhandsmelding",
                AppTitle = "Forhandsmelding",
                AllowedSteps = ["0-startside", "5-Byggherre", "8-bekreftelse"],
            }
        );

        using var provider = services.BuildServiceProvider();
        var connector = provider.GetRequiredService<IUmamiConnector>();

        using var writer = new StringWriter();
        connector.RenderScriptTag().WriteTo(writer, HtmlEncoder.Default);
        var html = writer.ToString();

        html.ShouldContain(
            "src=\"https://meldinger-umami-collect.arbeidstilsynet.no/altinn.js\""
        );
        html.ShouldContain($"data-website-id=\"{PocWebsiteId}\"");
        html.ShouldContain("data-allowed-origin=\"https://dat.apps.tt02.altinn.no\"");
        html.ShouldContain("data-app-path=\"/dat/forhandsmelding\"");
        html.ShouldContain("data-title=\"Forhandsmelding\"");
        html.ShouldContain("data-steps=\"0-startside,5-Byggherre,8-bekreftelse\"");
    }

    private static IConfiguration CreateConfiguration(IDictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private sealed class FakeHostEnvironment(
        string environmentName,
        string? contentRootPath = null
    ) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Altinn.Application";
        public string ContentRootPath { get; set; } = contentRootPath ?? AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
