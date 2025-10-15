# FeatureFlagProxy

A clean, simple abstraction over Unleash feature flags for .NET applications.

## 📖 Installation

```bash
dotnet add package Arbeidstilsynet.Common.FeatureFlagProxy
```

## 🧑‍💻 Usage

### Setup

First, configure your Unleash client and register the FeatureFlagProxy:

```csharp
public static IServiceCollection AddServices(
    this IServiceCollection services,
    WebApplicationBuilder builder)
{
    // Configure Unleash
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

    var unleash = new DefaultUnleash(unleashSettings);

    // Register FeatureFlagProxy
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

## 🏗️ Architecture

- **IFeatureFlagProxy**: Simple interface with `IsEnabled()` methods
- **FeatureFlagProxy**: Implementation using Unleash as backing service
- **FeatureFlagProxyBase**: Abstract base class for custom implementations
- **Model**: Contains `FeatureFlagContext` and `FeatureFlagResult` for structured data
- **Extensions**: Additional convenience methods for advanced scenariosAI COPYPASTA

```csharp
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

var unleash = new DefaultUnleash(unleashSettings);
services.AddFeatureFlagProxyWithUnleash(unleash); // ✨ Your new extension method
```
