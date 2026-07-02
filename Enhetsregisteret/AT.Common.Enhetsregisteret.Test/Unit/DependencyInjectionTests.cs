using System.Reflection;
using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class DependencyInjectionTests
{
    [Theory]
    [InlineData("Development", "https://data.ppe.brreg.no/")]
    [InlineData("Staging", "https://data.ppe.brreg.no/")]
    [InlineData("Production", "https://data.brreg.no/")]
    public void AddEnhetsregisteret_SetsCorrectBaseUrlBasedOnEnvironment(
        string envName,
        string expectedBaseUrl
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(envName);

        // Act
        services.AddEnhetsregisteret(environment);

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(DependencyInjectionExtensions.Clientkey);

        // Assert
        httpClient.ShouldNotBeNull();
        httpClient.BaseAddress!.AbsoluteUri.ShouldBe(expectedBaseUrl);
    }

    [Theory]
    [InlineData("Development", "https://data.ppe.brreg.no/")]
    [InlineData("Staging", "https://data.ppe.brreg.no/")]
    [InlineData("Production", "https://data.brreg.no/")]
    public void AddEnhetsregisteret_Overload_SetsCorrectBaseUrlBasedOnEnvironment(
        string envName,
        string expectedBaseUrl
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(envName);

        // Act
        services.AddEnhetsregisteret(environment, _ => { });

        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(DependencyInjectionExtensions.Clientkey);

        // Assert
        httpClient.ShouldNotBeNull();
        httpClient.BaseAddress!.AbsoluteUri.ShouldBe(expectedBaseUrl);
    }

    [Fact]
    public void AddEnhetsregisteret_AddsAllValidators()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedValidators = typeof(Arbeidstilsynet.Common.Enhetsregisteret.IAssemblyInfo)
            .Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IValidator)))
            .ToList();

        // Act
        services.AddEnhetsregisteret(Substitute.For<IWebHostEnvironment>());

        var serviceProvider = services.BuildServiceProvider();
        var validators = serviceProvider.GetServices<IValidator>();

        // Assert
        validators.ShouldAllBe(v => expectedValidators.Contains(v.GetType()));
    }

    [Fact]
    public void AddEnhetsregisteret_Overload_AddsAllValidators()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedValidators = typeof(Arbeidstilsynet.Common.Enhetsregisteret.IAssemblyInfo)
            .Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IValidator)))
            .ToList();

        // Act
        services.AddEnhetsregisteret(Substitute.For<IWebHostEnvironment>(), _ => { });

        var serviceProvider = services.BuildServiceProvider();
        var validators = serviceProvider.GetServices<IValidator>();

        // Assert
        validators.ShouldAllBe(v => expectedValidators.Contains(v.GetType()));
    }

    [Theory]
    [InlineData("Development", "https://data.ppe.brreg.no")]
    [InlineData("Production", "https://data.brreg.no")]
    public void AddEnhetsregisteret_RegistersEnhetsregisteretClient_WithBaseUrlWithoutTrailingSlash(
        string envName,
        string expectedBaseUrl
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(envName);

        // Act
        services.AddEnhetsregisteret(environment);

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<EnhetsregisteretClient>();

        // Assert
        client.ShouldNotBeNull();

        var requestAdapter = (IRequestAdapter)
            typeof(EnhetsregisteretClient)
                .GetProperty(
                    "RequestAdapter",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                )!
                .GetValue(client)!;
        requestAdapter.BaseUrl.ShouldBe(expectedBaseUrl);
    }

    [Fact]
    public void AddEnhetsregisteret_RegistersIEnhetsregisteret_AsAdapter()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEnhetsregisteret(Substitute.For<IWebHostEnvironment>());

        using var serviceProvider = services.BuildServiceProvider();
        var enhetsregisteret = serviceProvider
            .CreateScope()
            .ServiceProvider.GetRequiredService<IEnhetsregisteret>();

        // Assert
        enhetsregisteret.ShouldBeOfType<EnhetsregisteretAdapter>();
    }

    [Fact]
    public void AddEnhetsregisteret_Overload_RegistersIEnhetsregisteret_AsAdapter()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEnhetsregisteret(Substitute.For<IWebHostEnvironment>(), _ => { });

        using var serviceProvider = services.BuildServiceProvider();
        var enhetsregisteret = serviceProvider
            .CreateScope()
            .ServiceProvider.GetRequiredService<IEnhetsregisteret>();

        // Assert
        enhetsregisteret.ShouldBeOfType<EnhetsregisteretAdapter>();
    }

    [Fact]
    public void AddEnhetsregisteret_BrregApiBaseUrlOverwrite_TakesPrecedenceOverEnvironment()
    {
        // Arrange
        const string overrideUrl = "https://custom-brreg.example.com/";
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns("Production");

        // Act
        services.AddEnhetsregisteret(
            environment,
            config => config.BrregApiBaseUrlOverwrite = overrideUrl
        );

        using var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(DependencyInjectionExtensions.Clientkey);

        // Assert
        httpClient.BaseAddress!.AbsoluteUri.ShouldBe(overrideUrl);
    }

    [Fact]
    public void AddEnhetsregisteret_RegistersConfigAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new EnhetsregisteretConfig
        {
            BrregApiBaseUrlOverwrite = "https://custom-brreg.example.com/",
        };

        // Act
        services.AddEnhetsregisteret(Substitute.For<IWebHostEnvironment>(), config);

        using var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetRequiredService<EnhetsregisteretConfig>().ShouldBeSameAs(config);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddEnhetsregisteret_ResolvesClient_RegardlessOfCacheOption(bool cacheDisabled)
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns("Production");

        // Act
        services.AddEnhetsregisteret(
            environment,
            config => config.CacheOptions = new CacheOptions(Disabled: cacheDisabled)
        );

        using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider
            .CreateScope()
            .ServiceProvider.GetRequiredService<EnhetsregisteretClient>();

        // Assert
        client.ShouldNotBeNull();
    }
}
