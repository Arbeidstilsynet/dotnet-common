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

    // Register FeatureFlag service - uses modern ClientFactory internally
    services.AddFeatureFlag(unleashSettings);

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

    // Register FeatureFlag service using async ClientFactory
    await services.AddFeatureFlagAsync(unleashSettings);

    return services;
}
```

### Using in Your Code

Simple feature flag checking:

```csharp
using Arbeidstilsynet.Common.FeatureFlag.Model;

public class MyService
{
    private readonly IFeatureFlag _featureFlags;

    public MyService(IFeatureFlag featureFlags)
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

## 🌐 HTTP Endpoint

You can expose a MinimalAPI endpoint for checking feature flags via HTTP requests.

### Setup in Program.cs

```csharp
using Arbeidstilsynet.Common.FeatureFlag.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure services (see Setup section above)
builder.Services.AddFeatureFlag(unleashSettings);

var app = builder.Build();

// Map the feature flag endpoint
app.MapFeatureFlagEndpoint(); // Default route: POST /featureflag

// Or customize the route
app.MapFeatureFlagEndpoint("/api/features/check");

app.Run();
```

### Making HTTP Requests

**Request:**

```http
POST /featureflag
Content-Type: application/json

{
  "featureName": "my-feature",
  "context": {
    "userId": "user123",
    "sessionId": "session-abc",
    "environment": "production",
    "properties": {
      "region": "norway",
      "role": "admin"
    }
  }
}
```

**Response:**

```json
{
  "featureName": "my-feature",
  "isEnabled": true
}
```

**Simple request without context:**

```http
POST /featureflag
Content-Type: application/json

{
  "featureName": "simple-feature"
}
```

### Using with HttpClient

```csharp
using System.Net.Http.Json;

public class FeatureFlagClient
{
    private readonly HttpClient _httpClient;

    public FeatureFlagClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> IsFeatureEnabledAsync(string featureName, FeatureFlagContext? context = null)
    {
        var request = new FeatureFlagRequest
        {
            FeatureName = featureName,
            Context = context
        };

        var response = await _httpClient.PostAsJsonAsync("/featureflag", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<FeatureFlagResponse>();
        return result?.IsEnabled ?? false;
    }
}

// Usage
var isEnabled = await featureFlagClient.IsFeatureEnabledAsync("my-feature", new FeatureFlagContext
{
    UserId = "user123",
    Properties = new Dictionary<string, string> { { "region", "norway" } }
});
```

## 🗑️ Resource Management

The FeatureFlag service automatically manages the Unleash client lifecycle. When using `services.AddFeatureFlag(unleashSettings)` or `services.AddFeatureFlagAsync(unleashSettings)`, the Unleash client is created using the modern `ClientFactory` and automatically disposed by the DI container when the application shuts down.

No manual disposal is required - everything is handled automatically.

## 🏗️ Architecture

- **IFeatureFlag**: Simple interface with `IsEnabled()` method - a thin wrapper around Unleash
- **FeatureFlagImplementation**: Internal implementation that delegates to `IUnleash` with context mapping
- **FeatureFlagContext**: Custom model that extends Unleash's `UnleashContext` - consumers don't need to reference Unleash directly
- **Unleash as internal dependency**: Unleash is used internally but not exposed in public API surface

## 📦 Dependencies

Consumers of this package only need to reference `Arbeidstilsynet.Common.FeatureFlag`. The Unleash dependency is internal and doesn't need to be referenced in consuming applications.
