---
name: altinn
description: Integration with Altinn 3 APIs (instances, events, correspondence/meldinger, Maskinporten auth) using AT.Common.Altinn in this repository. Use this skill when consuming Altinn Storage, Altinn Events, Altinn Correspondence, or Maskinporten token exchange from a .NET backend.
license: MIT
metadata:
  domain: backend
  tags: altinn altinn3 maskinporten storage events correspondence meldinger dotnet integration
---

# Altinn Skill — Arbeidstilsynet/dotnet-common

`Arbeidstilsynet.Common.Altinn` (`AT.Common.Altinn.Publish`) provides typed HTTP clients and a higher-level adapter for interacting with Altinn 3 APIs, using Maskinporten for authentication.

---

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.Altinn
```

---

## Dependency Injection Setup

### Option A — Full adapter (recommended)

Registers `IAltinnAdapter`, `IAltinnMeldingerAdapter`, and all low-level clients.

```csharp
services.AddAltinnAdapter(builder.Environment, appSettings.MaskinportenConfiguration);
```

### Option B — Low-level clients only

```csharp
services.AddAltinnApiClients(builder.Environment, appSettings.MaskinportenConfiguration);
```

Both methods automatically resolve the correct Altinn base URLs based on environment:

| Environment | URLs used |
|-------------|-----------|
| Development | `http://local.altinn.cloud:5101/` (platform) / `http://local.altinn.cloud:5005/` (apps) |
| Non-production (other) | `https://platform.tt02.altinn.no/` |
| Production | `https://platform.altinn.no/` |

Override specific URLs by passing an `AltinnConfiguration`:

```csharp
services.AddAltinnAdapter(
    builder.Environment,
    appSettings.MaskinportenConfiguration,
    new AltinnConfiguration
    {
        StorageUrl = new Uri("https://custom-storage/"),
    }
);
```

---

## Configuration

### `MaskinportenConfiguration`

```json
{
  "Maskinporten": {
    "CertificatePrivateKey": "<base64-encoded RSA private key>",
    "CertificateChain": "<base64-encoded cert chain>",
    "KeyId": "<kid if pre-registered>",
    "IntegrationId": "<integration id>",
    "Scopes": ["altinn:instances.read", "altinn:instances.write"]
  }
}
```

```csharp
public record MaskinportenConfiguration
{
    public required string PrivateKey { get; init; }      // CertificatePrivateKey
    public string? CertificateChain { get; init; }
    public string? KeyId { get; init; }
    public required string IntegrationId { get; init; }
    public required string[] Scopes { get; init; }
    public Uri? MaskinportenUrl { get; init; }            // auto-resolved if null
}
```

### `AltinnConfiguration` (optional overrides)

```csharp
public record AltinnConfiguration
{
    public string OrgId { get; init; } = "dat";           // Arbeidstilsynet default
    public required Uri AuthenticationUrl { get; init; }
    public required Uri StorageUrl { get; init; }
    public required Uri EventUrl { get; init; }
    public required Uri CorrespondenceUrl { get; init; }
    public required Uri AppBaseUrl { get; init; }
}
```

---

## `IAltinnAdapter` — High-level adapter

Inject via DI after calling `AddAltinnAdapter`.

```csharp
public class MyService(IAltinnAdapter altinnAdapter)
{
    public async Task ProcessEvent(AltinnCloudEvent cloudEvent)
    {
        // Get a rich summary of an instance from a cloud event
        var summary = await altinnAdapter.GetSummary(cloudEvent);
    }

    public async Task ManageSubscription(SubscriptionRequestDto request)
    {
        // Create an event subscription for completed process events
        var subscription = await altinnAdapter.SubscribeForCompletedProcessEvents(request);

        // Verify it exists
        var existing = await altinnAdapter.GetAltinnSubscription(subscription.Id);

        // Remove it
        await altinnAdapter.UnsubscribeForCompletedProcessEvents(subscription);
    }

    public async Task ScanInstances(string appId)
    {
        // Get all non-completed instances (downloads data)
        var instances = await altinnAdapter.GetNonCompletedInstances(appId);

        // Get metadata only (no document download)
        var metadata = await altinnAdapter.GetMetadataForNonCompletedInstances(appId);
    }
}
```

---

## Low-level Clients

All clients are registered as `Transient` and use `AddStandardResilienceHandler` (built-in retry/circuit-breaker).

### `IAltinnStorageClient`

```csharp
// Get instance by address
var instance = await storageClient.GetInstance(new InstanceRequest { ... });

// Get instance from a cloud event
var instance = await storageClient.GetInstance(cloudEvent);

// Download data as a stream
var stream = await storageClient.GetInstanceData(new InstanceDataRequest { ... });
var stream = await storageClient.GetInstanceData(absoluteUri);

// Query instances
var result = await storageClient.GetInstances(new InstanceQueryParameters { ... });
```

### `IAltinnEventsClient`

```csharp
// Subscribe to events
var subscription = await eventsClient.Subscribe(new AltinnSubscriptionRequest { ... });

// Unsubscribe
var response = await eventsClient.Unsubscribe(subscriptionId);

// Get existing subscription
var subscription = await eventsClient.GetAltinnSubscription(subscriptionId);
```

### `IAltinnCorrespondenceClient`

```csharp
// Send a correspondence (Altinn melding), optionally with file attachments
var response = await correspondenceClient.InitializeCorrespondence(request, attachments);

// Retrieve an existing correspondence
var overview = await correspondenceClient.GetCorrespondence(guid);
```

### `IAltinnAppsClient`

Interact directly with Altinn App endpoints.

### `IAltinnAuthenticationClient`

Exchange Maskinporten tokens for Altinn tokens.

### `IMaskinportenClient`

Get raw Maskinporten tokens.

---

## Environment Behaviour

| Environment | Token provider |
|-------------|---------------|
| `IsDevelopment()` | `LocalAltinnTokenProvider` (uses local cert/key directly) |
| All other | `AltinnTokenProvider` (Maskinporten → Altinn token exchange) |

---

## Extension Methods

`AdapterExtensions`, `AltinnStorageClientExtensions`, `CorrespondenceRequestExtensions`, and `InstanceExtensions` provide convenience helpers for common operations on Altinn model objects.

---

## Adding Altinn Integration — Checklist

1. `dotnet add package Arbeidstilsynet.Common.Altinn`
2. Add `MaskinportenConfiguration` section to `appsettings.json` / secrets
3. Bind config with `builder.Configuration.GetRequired<MyAppSettings>()`
4. Register with `services.AddAltinnAdapter(builder.Environment, maskinportenConfig)`
5. Inject `IAltinnAdapter` (or specific low-level client) into your service
6. In development, configure local certificate/key for `LocalAltinnTokenProvider`
