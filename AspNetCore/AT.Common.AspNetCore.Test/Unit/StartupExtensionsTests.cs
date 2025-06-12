using Arbeidstilsynet.Common.AspNetCore.Extensions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace AT.Common.AspNetCore.Extensions.Test.Unit;

public class StartupExtensionsTests
{
    [Theory]
    [InlineData("PascalCase", "pascal-case")]
    [InlineData("LongAppName", "long-app-name")]
    [InlineData("LongAppNameWithNumbers123", "long-app-name-with-numbers123")]
    [InlineData("camelCase", "camel-case")]
    public void ConvertToOtelServiceName_ValidInput_ShouldReturnConvertedName(
        string input,
        string expectedOutput
    )
    {
        // Act
        var result = input.ConvertToOtelServiceName();

        // Assert
        result.ShouldBe(expectedOutput);
    }

    [Fact]
    public void ConfigureCors_ShouldAddCorsService()
    {
        // Arrange
        var services = CreateTestServiceCollection(isDevelopment: false);

        // Act
        services.ConfigureCors(isDevelopment: false);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<ICorsService>().ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureCors_InDevelopmentWithEmptyOrigins_ShouldAllowAnyOrigin()
    {
        // Arrange
        var services = CreateTestServiceCollection(isDevelopment: true);

        // Act
        var result = services.ConfigureCors(allowedOrigins: [], isDevelopment: true);

        // Assert
        result.ShouldBe(services);

        var serviceProvider = services.BuildServiceProvider();
        var corsService = serviceProvider.GetRequiredService<ICorsService>();
        var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = "https://example.com";
        httpContext.Request.Method = "GET";

        var policy = corsOptions.Value.GetPolicy(corsOptions.Value.DefaultPolicyName ?? "");
        var corsResult = corsService.EvaluatePolicy(httpContext, policy!);
        corsResult.IsOriginAllowed.ShouldBeTrue();
    }

    [Fact]
    public void ConfigureCors_WithSpecificOrigins_ShouldConfigureWithOrigins()
    {
        // Arrange
        var services = CreateTestServiceCollection(isDevelopment: false);
        var allowedOrigins = new[] { "https://example.com", "https://test.com" };

        // Act
        var result = services.ConfigureCors(allowedOrigins);

        // Assert
        result.ShouldBe(services);

        var serviceProvider = services.BuildServiceProvider();
        var corsService = serviceProvider.GetRequiredService<ICorsService>();
        var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = "https://example.com";
        httpContext.Request.Method = "POST";

        var policy = corsOptions.Value.GetPolicy(corsOptions.Value.DefaultPolicyName ?? "");
        var corsResult = corsService.EvaluatePolicy(httpContext, policy!);
        corsResult.IsOriginAllowed.ShouldBeTrue();

        httpContext.Request.Headers.Origin = "https://notallowed.com";
        corsResult = corsService.EvaluatePolicy(httpContext, policy!);
        corsResult.IsOriginAllowed.ShouldBeFalse();
    }

    [Fact]
    public void ConfigureCors_WithOriginsAndCredentials_ShouldConfigureWithCredentials()
    {
        // Arrange
        var services = CreateTestServiceCollection(isDevelopment: false);
        var allowedOrigins = new[] { "https://example.com" };

        // Act
        var result = services.ConfigureCors(allowedOrigins, allowCredentials: true);

        // Assert
        result.ShouldBe(services);

        var serviceProvider = services.BuildServiceProvider();
        var corsService = serviceProvider.GetRequiredService<ICorsService>();
        var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = "https://example.com";
        httpContext.Request.Method = "POST";

        var policy = corsOptions.Value.GetPolicy(corsOptions.Value.DefaultPolicyName ?? "");
        var corsResult = corsService.EvaluatePolicy(httpContext, policy!);
        corsResult.IsOriginAllowed.ShouldBeTrue();
        corsResult.SupportsCredentials.ShouldBeTrue();
    }

    [Fact]
    public void ConfigureCors_InProductionWithDefaultParameters_ShouldConfigureRestrictiveCors()
    {
        // Arrange
        var services = CreateTestServiceCollection(isDevelopment: false);

        // Act
        var result = services.ConfigureCors();

        // Assert
        result.ShouldBe(services);

        var serviceProvider = services.BuildServiceProvider();
        var corsService = serviceProvider.GetRequiredService<ICorsService>();
        var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = "https://example.com";
        httpContext.Request.Method = "GET";

        var policy = corsOptions.Value.GetPolicy(corsOptions.Value.DefaultPolicyName ?? "");
        var corsResult = corsService.EvaluatePolicy(httpContext, policy!);
        corsResult.IsOriginAllowed.ShouldBeFalse();
    }

    [Fact]
    public void ConfigureCors_InProductionWithEmptyOrigins_ShouldConfigureRestrictiveCors()
    {
        // Arrange
        var services = CreateTestServiceCollection(isDevelopment: false);

        // Act
        var result = services.ConfigureCors(allowedOrigins: []);

        // Assert
        result.ShouldBe(services);

        var serviceProvider = services.BuildServiceProvider();
        var corsService = serviceProvider.GetRequiredService<ICorsService>();
        var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = "https://example.com";
        httpContext.Request.Method = "GET";

        var policy = corsOptions.Value.GetPolicy(corsOptions.Value.DefaultPolicyName ?? "");
        var corsResult = corsService.EvaluatePolicy(httpContext, policy!);
        corsResult.IsOriginAllowed.ShouldBeFalse();
    }

    [Fact]
    public void ConfigureCors_WithNullOriginsInDevelopment_ShouldAllowAnyOrigin()
    {
        // Arrange
        var services = CreateTestServiceCollection(isDevelopment: true);

        // Act
        var result = services.ConfigureCors(allowedOrigins: null, isDevelopment: true);

        // Assert
        result.ShouldBe(services);

        var serviceProvider = services.BuildServiceProvider();
        var corsService = serviceProvider.GetRequiredService<ICorsService>();
        var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Origin = "https://example.com";
        httpContext.Request.Method = "GET";

        var policy = corsOptions.Value.GetPolicy(corsOptions.Value.DefaultPolicyName ?? "");
        var corsResult = corsService.EvaluatePolicy(httpContext, policy!);
        corsResult.IsOriginAllowed.ShouldBeTrue();
    }

    private static ServiceCollection CreateTestServiceCollection(bool isDevelopment)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IWebHostEnvironment>(CreateMockEnvironment(isDevelopment));
        return services;
    }

    private static IWebHostEnvironment CreateMockEnvironment(bool isDevelopment)
    {
        var environment = new TestWebHostEnvironment
        {
            EnvironmentName = isDevelopment ? Environments.Development : Environments.Production,
        };
        return environment;
    }

    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "TestApp";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
    }
}
