# FeatureFlag

A clean, simple abstraction over Unleash feature flags for .NET applications.

## 📖 Installation

```bash
dotnet add package Arbeidstilsynet.Common.FeatureFlag
```

## 🧑‍💻 Usage

### Setup

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

#### Option 2: Async setup (if you prefer async initialization)

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

### Using in Your Code

Simple feature flag checking:

```csharp
using Arbeidstilsynet.Common.FeatureFlagProxy.Model;

public class MyService
{
    private readonly IFeatureFlagProxy _featureFlags;

    public MyService(IFeatureFlagProxy featureFlags)
    {
        _featureFlags = featureFlags;
    }

    public async Task DoSomething()
    {
        // Simple check - no context needed
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
        var userContext = new FeatureFlagContext { UserId = "user123" };
        if (_featureFlags.IsEnabled("user-specific-feature", userContext))
        {
            await ExecuteUserSpecificFeature();
        }

        // With full context - all properties available
        var fullContext = new FeatureFlagContext
        {
            UserId = "user123",
            SessionId = "session-abc",
            RemoteAddress = "192.168.1.1",
            Environment = "production",
            AppName = "MyApp",
            Properties = new Dictionary<string, string>
            {
                { "region", "norway" },
                { "role", "admin" },
                { "subscription", "premium" }
            }
        };

        if (_featureFlags.IsEnabled("region-feature", fullContext))
        {
            await ExecuteRegionSpecificFeature();
        }
    }
}
```

### Using Extension Methods

```csharp

```

## 🗑️ Resource Management

The FeatureFlagProxy automatically manages the Unleash client lifecycle. When using `services.AddFeatureFlagProxy(unleashSettings)` or `services.AddFeatureFlagProxyAsync(unleashSettings)`, the Unleash client is created using the modern `ClientFactory` and automatically disposed by the DI container when the application shuts down.

No manual disposal is required - everything is handled automatically.

## 🏗️ Architecture

- **IFeatureFlagProxy**: Simple interface with `IsEnabled()` method - a thin wrapper around Unleash
- **FeatureFlagProxyImplementation**: Internal implementation that delegates to `IUnleash` with context mapping
- **FeatureFlagContext**: Custom model that mirrors Unleash's context - consumers don't need to reference Unleash directly
- **Unleash as internal dependency**: Unleash is used internally but not exposed in public API surface

## 📦 Dependencies

Consumers of this package only need to reference `Arbeidstilsynet.Common.FeatureFlagProxy`. The Unleash dependency is internal and doesn't need to be referenced in consuming applications.
