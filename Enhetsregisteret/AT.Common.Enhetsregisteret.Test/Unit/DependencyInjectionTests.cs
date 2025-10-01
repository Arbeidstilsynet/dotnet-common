using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class DependencyInjectionTests
{
    [Theory]
    [InlineData("Development", "https://data.ppe.brreg.no/")]
    [InlineData("Staging", "https://data.ppe.brreg.no/")]
    [InlineData("Production", "https://data.brreg.no/")]
    public void AddEnhetsregisteret_SetsCorrectBaseUrlBasedOnEnvironment(string envName, string expectedBaseUrl)
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
    public void AddEnhetsregisteret_Overload_SetsCorrectBaseUrlBasedOnEnvironment(string envName, string expectedBaseUrl)
    {
        // Arrange
        var services = new ServiceCollection();
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(envName);

        // Act
        services.AddEnhetsregisteret(environment, _ => {});

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
        var expectedValidators =
            typeof(Enhetsregisteret.IAssemblyInfo).Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IValidator))).ToList();

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
        var expectedValidators =
            typeof(Enhetsregisteret.IAssemblyInfo).Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IValidator))).ToList();

        // Act
        services.AddEnhetsregisteret(Substitute.For<IWebHostEnvironment>(), _ => {});

        var serviceProvider = services.BuildServiceProvider();
        var validators = serviceProvider.GetServices<IValidator>();
        
        // Assert
        validators.ShouldAllBe(v => expectedValidators.Contains(v.GetType()));
    }
}