# Introduction

A feature flag library for .NET applications using Unleash as the backing service. This package provides a simple interface for checking feature flags with support for context-based evaluation.

## 📖 Installation

To install the FeatureFlags package, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.FeatureFlags
```

## 🧑‍💻 Usage

### Configuration

Add feature flag settings to your `appsettings.json`:

```json
{
  "FeatureFlags": {
    "Url": "https://unleash.example.com/api",
    "ApiKey": "your-api-key-here",
    "AppName": "my-application"
  }
}
```

### Service Registration

Add to your service collection in `Program.cs` or `Startup.cs`:

```csharp
using Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;
using Arbeidstilsynet.Common.FeatureFlags.Model;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration
var featureFlagSettings = builder.Configuration
    .GetSection("FeatureFlags")
    .Get<FeatureFlagSettings>();

// Register feature flags service
builder.Services.AddFeatureFlags(
    builder.Environment,
    featureFlagSettings
);

var app = builder.Build();
```

> **Note:** If `featureFlagSettings` is `null` or has missing configuration, the service will automatically use `FakeUnleash` for testing purposes.

### Using Feature Flags in Your Code

Inject `IFeatureFlags` into your services or controllers:

```csharp
using Arbeidstilsynet.Common.FeatureFlags.Model;
using Arbeidstilsynet.Common.FeatureFlags.Ports;

public class MyService
{
    private readonly IFeatureFlags _featureFlags;

    public MyService(IFeatureFlags featureFlags)
    {
        _featureFlags = featureFlags;
    }

    public void DoSomething()
    {
        // Simple feature check
        if (_featureFlags.IsEnabled("my-new-feature"))
        {
            // New feature logic
        }
        else
        {
            // Old logic
        }
    }

    public void DoSomethingWithContext(string userId)
    {
        // Feature check with context
        var context = new FeatureFlagContext
        {
            UserId = userId,
            Environment = "production"
        };

        if (_featureFlags.IsEnabled("user-specific-feature", context))
        {
            // Context-aware feature logic
        }
    }
}
```

### Mapping the Feature Flag Endpoint

You can expose an HTTP endpoint to check feature flags remotely:

```csharp
using Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;

var app = builder.Build();

app.UseRouting();

// Map with default route: POST /feature-flags
app.MapFeatureFlagEndpoint();

// Or with a custom route:
app.MapFeatureFlagEndpoint("/api/features");

app.Run();
```

**Example request:**

```bash
POST /feature-flags
Content-Type: application/json

{
  "featureName": "my-new-feature",
  "context": {
    "userId": "user123",
    "environment": "production"
  }
}
```

**Example response:**

```json
{
  "isEnabled": true
}
```

## 🧪 Testing

For testing purposes, when no configuration is provided, the service automatically uses `FakeUnleash`:

```csharp
// In your test setup
services.AddFeatureFlags(webHostEnvironment, null);
```

You can also configure specific features to be enabled in tests:

```csharp
var fakeUnleash = new FakeUnleash();
fakeUnleash.Enable("test-feature");
services.AddSingleton<IUnleash>(fakeUnleash);
services.AddFeatureFlags(webHostEnvironment, null);
```
