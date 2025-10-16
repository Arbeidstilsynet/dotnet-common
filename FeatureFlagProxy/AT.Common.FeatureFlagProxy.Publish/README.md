# FeatureFlagProxy

A clean, simple abstraction over Unleash feature flags for .NET applications.

## 📖 Installation

```bash
dotnet add package Arbeidstilsynet.Common.FeatureFlagProxy
```

## 🧑‍💻 Usage

### Setup

### Setup Options

#### Option 1: Simple setup with UnleashSettings (Recommended)

```csharp
public static IServiceCollection AddServices(
    this IServiceCollection services,
    WebApplicationBuilder builder)
{
    // Configure Unleash settings
    var unleashSettings = new UnleashSettings
    {
        AppName = IAssemblyInfo.AppName,
        InstanceTag = builder.Environment.EnvironmentName,
        UnleashApi = new Uri(
            builder.Configuration["Unleash:Url"]
                ?? throw new InvalidOperationException("Unleash:Url is not configured")
        ),
        CustomHttpHeaders = new Dictionary<string, string>
        {
            {
                "Authorization",
                builder.Configuration["Unleash:ApiKey"]
                    ?? throw new InvalidOperationException("Unleash:ApiKey is not configured")
            },
        },
    };

    // Register FeatureFlagProxy - uses modern ClientFactory internally
    services.AddFeatureFlagProxy(unleashSettings);

    return services;
}
```

#### Option 1b: Async setup (if you prefer async initialization)

```csharp
public static async Task<IServiceCollection> AddServices(
    this IServiceCollection services,
    WebApplicationBuilder builder)
{
    var unleashSettings = new UnleashSettings
    {
        // ... same configuration as above
    };

    // Register FeatureFlagProxy using async ClientFactory
    await services.AddFeatureFlagProxyAsync(unleashSettings);

    return services;
}
```

#### Option 2: Advanced setup with custom IUnleash instance

```csharp
public static IServiceCollection AddServices(
    this IServiceCollection services,
    WebApplicationBuilder builder)
{
    // Configure Unleash settings
    var unleashSettings = new UnleashSettings
    {
        AppName = IAssemblyInfo.AppName,
        InstanceTag = builder.Environment.EnvironmentName,
        UnleashApi = new Uri(
            builder.Configuration["Unleash:Url"]
                ?? throw new InvalidOperationException("Unleash:Url is not configured")
        ),
        CustomHttpHeaders = new Dictionary<string, string>
        {
            {
                "Authorization",
                builder.Configuration["Unleash:ApiKey"]
                    ?? throw new InvalidOperationException("Unleash:ApiKey is not configured")
            },
        },
    };

    // Create custom Unleash instance using modern ClientFactory (or use legacy DefaultUnleash if needed)
    var unleash = new UnleashClientFactory().CreateClient(unleashSettings);
    // Alternative: var unleash = new DefaultUnleash(unleashSettings); // Legacy approach

    // Register FeatureFlagProxy with custom instance
    services.AddFeatureFlagProxy(unleash);

    return services;
}
```

### Using in Your Code

Simple feature flag checking:

```csharp
public class MyService
{
    private readonly IFeatureFlagProxy _featureFlags;

    public MyService(IFeatureFlagProxy featureFlags)
    {
        _featureFlags = featureFlags;
    }

    public async Task DoSomething()
    {
        // Simple check
        if (_featureFlags.IsEnabled("my-feature"))
        {
            // Execute new feature
            await ExecuteNewFeature();
        }
        else
        {
            // Execute default behavior
            await ExecuteDefaultBehavior();
        }

        // With user context
        if (_featureFlags.IsEnabled("user-specific-feature", "user123"))
        {
            await ExecuteUserSpecificFeature();
        }

        // With additional context
        var properties = new Dictionary<string, string>
        {
            { "region", "norway" },
            { "role", "admin" }
        };

        if (_featureFlags.IsEnabled("region-feature", "user123", properties))
        {
            await ExecuteRegionSpecificFeature();
        }
    }
}
```

### Using Extension Methods

```csharp
using Arbeidstilsynet.Common.FeatureFlagProxy.Extensions;
using Arbeidstilsynet.Common.FeatureFlagProxy.Model;

public class AdvancedService
{
    private readonly IFeatureFlagProxy _featureFlags;

    public AdvancedService(IFeatureFlagProxy featureFlags)
    {
        _featureFlags = featureFlags;
    }

    public async Task DoAdvancedStuff()
    {
        // Using context object
        var context = new FeatureFlagContext
        {
            UserId = "user123",
            Properties = new Dictionary<string, string>
            {
                { "subscription", "premium" },
                { "region", "europe" }
            }
        };

        if (_featureFlags.IsEnabled("premium-feature", context))
        {
            await ExecutePremiumFeature();
        }

        // Get detailed result
        var result = _featureFlags.GetResult("analytics-feature", context);

        Console.WriteLine($"Feature '{result.FeatureName}' is {(result.IsEnabled ? "enabled" : "disabled")}");
    }
}
```

## 🗑️ Resource Management

### Automatic Disposal (Recommended - Option 1 & 1b)

When using `services.AddFeatureFlagProxy(unleashSettings)` or `services.AddFeatureFlagProxyAsync(unleashSettings)`, the Unleash client is created using the modern `ClientFactory` and automatically managed by the DI container. It will be properly disposed when the application shuts down.

### Manual Disposal (Option 2)

When providing your own `IUnleash` instance, **you are responsible for disposal**:

```csharp
// ⚠️ Manual disposal required
var unleash = new DefaultUnleash(unleashSettings);
services.AddFeatureFlagProxy(unleash);

// In application shutdown or when disposing the service provider:
unleash.Dispose(); // You must handle this
```

**Important**: If you register your own `IUnleash` instance, ensure it's properly disposed to avoid resource leaks. The DI container will only dispose objects it creates itself.

## 🏗️ Architecture

- **IFeatureFlagProxy**: Simple interface with `IsEnabled()` methods
- **FeatureFlagProxy**: Implementation using Unleash as backing service
- **FeatureFlagProxyBase**: Abstract base class for custom implementations
- **Model**: Contains `FeatureFlagContext` and `FeatureFlagResult` for structured data
- **Extensions**: Additional convenience methods for advanced scenarios
