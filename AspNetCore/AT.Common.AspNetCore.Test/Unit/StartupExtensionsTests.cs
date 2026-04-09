using Arbeidstilsynet.Common.AspNetCore.DependencyInjection;
using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

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
    public void AddMemoryCachedClient_ShouldAddMemoryCacheAndHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var clientBuilder = services.AddMemoryCachedClient("TestClient");

        // Assert
        services.ShouldContain(s => s.ServiceType == typeof(IMemoryCache));
        services.ShouldContain(s => s.ServiceType == typeof(IHttpClientFactory));
        clientBuilder.ShouldNotBeNull();
    }

    [Fact]
    public void AddMemoryCachedClient_WithConfiguration_ShouldAddMemoryCacheAndHttpClientWithConfig()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var clientBuilder = services.AddMemoryCachedClient("TestClient", (_) => { });

        // Assert
        services.ShouldContain(s => s.ServiceType == typeof(IMemoryCache));
        services.ShouldContain(s => s.ServiceType == typeof(IHttpClientFactory));
        clientBuilder.ShouldNotBeNull();
    }

    [Fact]
    public void AddMemoryCachedClient_WithServiceProviderConfiguration_ShouldAddMemoryCacheAndHttpClientWithConfig()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var clientBuilder = services.AddMemoryCachedClient("TestClient", (_, _) => { });

        // Assert
        services.ShouldContain(s => s.ServiceType == typeof(IMemoryCache));
        services.ShouldContain(s => s.ServiceType == typeof(IHttpClientFactory));
        clientBuilder.ShouldNotBeNull();
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

    [Fact]
    public async Task AddStartupChecks_MultipleRegistrations_AllChecksAreExecuted()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<StartupHealthCheck>();

        var check1Executed = false;
        var check2Executed = false;
        var check3Executed = false;

        services.AddStartupChecks(_ => [Task.Run(() => check1Executed = true)]);
        services.AddStartupChecks(_ => [Task.Run(() => check2Executed = true)]);
        services.AddStartupChecks(_ => [Task.Run(() => check3Executed = true)]);

        var serviceProvider = services.BuildServiceProvider();
        var backgroundService =
            serviceProvider.GetRequiredService<IHostedService>() as BackgroundService;

        // Act
        await backgroundService!.StartAsync(TestContext.Current.CancellationToken);
        await backgroundService.ExecuteTask!;
        await backgroundService.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        check1Executed.ShouldBeTrue();
        check2Executed.ShouldBeTrue();
        check3Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task AddStartupChecks_MultipleRegistrations_SetsHealthCheckToCompleted()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<StartupHealthCheck>();

        services.AddStartupChecks(_ => [Task.CompletedTask]);
        services.AddStartupChecks(_ => [Task.CompletedTask]);

        var serviceProvider = services.BuildServiceProvider();
        var healthCheck = serviceProvider.GetRequiredService<StartupHealthCheck>();
        var backgroundService =
            serviceProvider.GetRequiredService<IHostedService>() as BackgroundService;

        healthCheck.StartupCompleted.ShouldBeFalse();

        // Act
        await backgroundService!.StartAsync(TestContext.Current.CancellationToken);
        await backgroundService.ExecuteTask!;
        await backgroundService.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        healthCheck.StartupCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task AddStartupChecks_MultipleRegistrations_ExecutesInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<StartupHealthCheck>();

        var executionOrder = new List<int>();

        services.AddStartupChecks(_ => [Task.Run(() => executionOrder.Add(1))]);
        services.AddStartupChecks(_ => [Task.Run(() => executionOrder.Add(2))]);
        services.AddStartupChecks(_ => [Task.Run(() => executionOrder.Add(3))]);

        var serviceProvider = services.BuildServiceProvider();
        var backgroundService =
            serviceProvider.GetRequiredService<IHostedService>() as BackgroundService;

        // Act
        await backgroundService!.StartAsync(TestContext.Current.CancellationToken);
        await backgroundService.ExecuteTask!;
        await backgroundService.StopAsync(TestContext.Current.CancellationToken);

        // Assert
        executionOrder.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void AddStandardAuth_DisableAuthTrue_RegistersPermissiveAuthorizationPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var authConfig = new AuthConfiguration
        {
            DisableAuth = true,
            TenantId = "fake-tenant",
            ClientId = "fake-client",
            Scope = "fake-scope",
        };

        // Act
        services.AddStandardAuth(authConfig);

        // Assert — no authentication scheme should be registered
        services.ShouldNotContain(s => s.ServiceType == typeof(IAuthenticationService));

        var serviceProvider = services.BuildServiceProvider();
        var authOptions = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>();
        var defaultPolicy = authOptions.Value.DefaultPolicy;

        defaultPolicy.ShouldNotBeNull();
        defaultPolicy.Requirements.Count.ShouldBe(1);
        defaultPolicy.AuthenticationSchemes.ShouldBeEmpty();
    }

    [Fact]
    public async Task AddStandardAuth_DisableAuthFalse_RegistersJwtBearerAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var authConfig = new AuthConfiguration
        {
            DisableAuth = false,
            TenantId = "fake-tenant",
            ClientId = "fake-client",
            Scope = "fake-scope",
        };

        // Act
        services.AddStandardAuth(authConfig);

        // Assert — JWT bearer authentication should be registered
        var serviceProvider = services.BuildServiceProvider();
        var authSchemeProvider =
            serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = await authSchemeProvider.GetSchemeAsync(
            JwtBearerDefaults.AuthenticationScheme
        );

        scheme.ShouldNotBeNull();
        scheme.Name.ShouldBe(JwtBearerDefaults.AuthenticationScheme);
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
