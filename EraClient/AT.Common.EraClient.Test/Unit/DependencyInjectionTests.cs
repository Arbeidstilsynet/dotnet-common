using Arbeidstilsynet.Common.EraClient.Adapters;
using Arbeidstilsynet.Common.EraClient.Adapters.DependencyInjection;
using Arbeidstilsynet.Common.EraClient.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.EraClient.Test;

public class DependencyInjectionTests
{
    private static List<ServiceDescriptor> ExpectedServices =
    [
        ServiceDescriptor.Transient<IAuthenticationClient, AuthenticationClient>(),
    ];

    [Fact]
    public void AddEraAdapter_WhenCalledWithOptions_AddRequiredServices()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddEraAdapter(
            Substitute.For<IHostEnvironment>(),
            options => options.AuthenticationUrl = "https://test-auth-url.com"
        );
        // assert
        services.AssertContains(ExpectedServices);
    }

    [Fact]
    public void AddEraAdapter_WhenCalledWithConfiguration_AddRequiredServices()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddEraAdapter(
            Substitute.For<IHostEnvironment>(),
            new Adapters.DependencyInjection.EraClientConfiguration
            {
                AuthenticationUrl = "https://test-auth-url.com",
            }
        );
        // assert
        services.AssertContains(ExpectedServices);
    }
}

internal static class DependencyTestExtensions
{
    internal static void AssertContains(
        this IServiceCollection services,
        List<ServiceDescriptor> expectedServices
    )
    {
        foreach (var service in expectedServices)
        {
            services.ShouldContain(m =>
                m.ServiceType == service.ServiceType
                && m.ImplementationType == service.ImplementationType
                && m.Lifetime == service.Lifetime
            );
        }
    }
}
