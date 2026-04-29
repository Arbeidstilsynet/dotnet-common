---
name: feature-flags
description: Feature flag evaluation backed by Unleash using AT.Common.FeatureFlags. Use this skill when adding, checking, or testing feature flags in a .NET application, exposing a feature flag HTTP endpoint, or configuring context-based flag evaluation.
license: MIT
metadata:
  domain: backend
  tags: feature-flags unleash dotnet toggles context testing
---

# FeatureFlags Skill — Arbeidstilsynet/dotnet-common

`Arbeidstilsynet.Common.FeatureFlags` (`AT.Common.FeatureFlags.Publish`) wraps the Unleash client in a simple `IFeatureFlags` interface with context support, automatic fallback for testing, and an optional HTTP endpoint.

---

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.FeatureFlags
```

---

## Configuration

Add to `appsettings.json`:

```json
{
  "FeatureFlags": {
    "Url": "https://unleash.example.com/api",
    "ApiKey": "your-api-key-here",
    "AppName": "my-application"
  }
}
```

---

## Dependency Injection Setup

```csharp
using Arbeidstilsynet.Common.FeatureFlags.DependencyInjection;
using Arbeidstilsynet.Common.FeatureFlags.Model;

var featureFlagSettings = builder.Configuration
    .GetSection("FeatureFlags")
    .Get<FeatureFlagSettings>() ?? new FeatureFlagSettings();

builder.Services.AddFeatureFlags(featureFlagSettings);
```

> **Note:** If `Url` or `ApiKey` is empty in `featureFlagSettings`, the service automatically uses `FakeUnleash` — all flags are disabled by default. This keeps tests and local development working without an Unleash server.

---

## Checking Feature Flags

Inject `IFeatureFlags` into any service or controller.

```csharp
using Arbeidstilsynet.Common.FeatureFlags.Ports;

public class MyService(IFeatureFlags featureFlags)
{
    public void DoSomething()
    {
        if (featureFlags.IsEnabled("my-new-feature"))
        {
            // New code path
        }
        else
        {
            // Old code path
        }
    }
}
```

### Context-based evaluation

Pass a `FeatureFlagContext` to enable user-specific or group-specific rollouts:

```csharp
var context = new FeatureFlagContext
{
    UserId = userId,
};

if (featureFlags.IsEnabled("user-specific-feature", context))
{
    // Only enabled for this user
}
```

---

## HTTP Endpoint

Expose a feature flag check endpoint for frontend or cross-service consumers:

```csharp
// Default route: POST /featureflag
app.MapFeatureFlagEndpoint();

// Custom route
app.MapFeatureFlagEndpoint("/api/features");
```

**Request:**

```http
POST /featureflag
Content-Type: application/json

{
  "featureName": "my-new-feature",
  "context": {
    "userId": "user123"
  }
}
```

**Response:**

```json
{
  "featureName": "my-new-feature",
  "isEnabled": true
}
```

---

## Testing

When `Url` or `ApiKey` is empty, `FakeUnleash` is registered automatically. Toggle flags directly via the `FakeUnleash` instance in tests:

```csharp
services.AddFeatureFlags(new FeatureFlagSettings());

// Resolve the FakeUnleash instance and enable a flag
var fakeUnleash = (FakeUnleash)serviceProvider.GetRequiredService<IUnleash>();
fakeUnleash.SetToggle("test-feature", true);
```

Or rely on the auto-fallback (all flags disabled by default):

```csharp
// All flags disabled; call SetToggle on the resolved FakeUnleash to override per test
services.AddFeatureFlags(new FeatureFlagSettings());
```

---

## `IFeatureFlags` Interface

```csharp
public interface IFeatureFlags
{
    bool IsEnabled(string featureName, FeatureFlagContext? context = null);
}
```

---

## Adding a Feature Flag — Checklist

1. Define the flag in Unleash (name must match the string passed to `IsEnabled`)
2. Wrap the new code path with `if (featureFlags.IsEnabled("flag-name"))`
3. In tests, use `fakeUnleash.SetToggle("flag-name", true)` to activate the flag
4. Remove the flag and old code path once the feature is fully rolled out
