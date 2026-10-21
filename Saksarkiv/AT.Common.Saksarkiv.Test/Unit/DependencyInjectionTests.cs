using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection;
using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Saksarkiv.Test.Unit;

public class DependencyInjectionTests
{
    [Theory]
    [InlineData("POST", "/api/v3/saker", true)]
    [InlineData("POST", "/apiv2/sak/opprett", true)]
    [InlineData("POST", "/apiv2/sak/finnEnSak", true)]
    [InlineData("GET", "/api/health/authPing", true)]
    [InlineData("GET", "/api/health/ping", true)]
    [InlineData("GET", "/api/v3/saker/abc", false)]
    [InlineData("GET", "/apiv2/sak/finnEnSak", false)]
    [InlineData(null, null, false)]
    public void ShouldSkipRetryForRequest_ReturnsExpectedResult(
        string? httpMethod,
        string? requestPath,
        bool expectedResult
    )
    {
        var method = httpMethod is null ? null : new HttpMethod(httpMethod);
        var result = DependencyInjectionExtensions.ShouldSkipRetryForRequest(method, requestPath);

        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void AddSaksarkivClient_RegistersV3ClientAndConfig()
    {
        var services = new ServiceCollection();
        var tokenProvider = Substitute.For<Ports.ISaksarkivTokenProvider>();
        services.AddScoped(_ => tokenProvider);

        var config = new SaksarkivConfiguration
        {
            BaseUrl = "https://saksarkiv.example.com/",
            Scope = "api://scope",
        };

        services.AddSaksarkivClient(config);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<SaksarkivConfiguration>().ShouldBe(config);
        provider.GetRequiredService<V3.SaksarkivClientV3>().ShouldNotBeNull();
        provider
            .GetRequiredService<Implementation.ISaksarkivHealthPinger>()
            .ShouldBeOfType<Implementation.SaksarkivHealthPinger>();

        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("SaksarkivHttpClient");
        httpClient.BaseAddress.ShouldBe(new Uri(config.BaseUrl));
    }

    [Fact]
    public void AddSaksarkivClientV2_RegistersV2ClientOnDemand()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Substitute.For<Ports.ISaksarkivTokenProvider>());

        var config = new SaksarkivConfiguration
        {
            BaseUrl = "https://saksarkiv.example.com/",
            Scope = "api://scope",
        };

        services.AddSaksarkivClientV2(config);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<SaksarkivClient>().ShouldNotBeNull();
    }

    [Fact]
    public void AddSaksarkivClient_AndV2_CanBeCombined()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Substitute.For<Ports.ISaksarkivTokenProvider>());

        var config = new SaksarkivConfiguration
        {
            BaseUrl = "https://saksarkiv.example.com/",
            Scope = "api://scope",
        };

        services.AddSaksarkivClient(config).AddSaksarkivClientV2(config);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<V3.SaksarkivClientV3>().ShouldNotBeNull();
        provider.GetRequiredService<SaksarkivClient>().ShouldNotBeNull();
    }

    [Fact]
    public void AddSaksarkivClient_AllowsResilienceCustomization()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Substitute.For<Ports.ISaksarkivTokenProvider>());

        HttpStandardResilienceOptions? capturedOptions = null;

        services.AddSaksarkivClient(
            new SaksarkivConfiguration
            {
                BaseUrl = "https://saksarkiv.example.com/",
                Scope = "api://scope",
            },
            options =>
            {
                options.Retry.MaxRetryAttempts = 5;
                capturedOptions = options;
            }
        );

        using var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        _ = httpClientFactory.CreateClient("SaksarkivHttpClient");

        capturedOptions.ShouldNotBeNull();
        capturedOptions.Retry.MaxRetryAttempts.ShouldBe(5);
    }
}
