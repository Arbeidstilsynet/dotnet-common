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
    [InlineData("POST", "/apiv2/sak/opprett", true)]
    [InlineData("POST", "/apiv2/sak/finnEnSak", true)]
    [InlineData("GET", "/apiv2/health", true)]
    [InlineData("GET", "/apiv2/health/pong", true)]
    [InlineData("GET", "/apiv2/health/vannskille", true)]
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
    public void AddSaksarkivClient_RegistersClientAndConfig()
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
        provider.GetRequiredService<SaksarkivClient>().ShouldNotBeNull();

        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("SaksarkivHttpClient");
        httpClient.BaseAddress.ShouldBe(new Uri(config.BaseUrl));
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
