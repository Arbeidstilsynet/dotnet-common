---
name: era-client
description: Client for accessing Arbeidstilsynet's external ERA APIs (OAuth protected) using AT.Common.EraClient. Use this skill when integrating with ERA from an Altinn application or other external context where only the public ERA API is accessible.
license: MIT
metadata:
  domain: backend
  tags: era-client era oauth authentication asbest dotnet altinn
---

# EraClient Skill — Arbeidstilsynet/dotnet-common

`Arbeidstilsynet.Common.EraClient` (`AT.Common.EraClient.Publish`) is a typed HTTP client for Arbeidstilsynet's external ERA APIs, handling OAuth authentication. It is designed for use in Altinn applications and other contexts where only the external ERA API endpoint is reachable.

For access to client credentials, contact `#team-godkjenning`.

---

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.EraClient
```

---

## Dependency Injection Setup

```csharp
using Arbeidstilsynet.Common.EraClient.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEraAdapter();
```

### `AddEraAdapter` Overloads

| Overload | Description |
|----------|-------------|
| `AddEraAdapter()` | Registers all ERA clients with default configuration |
| `AddEraAdapter(Action<EraClientConfiguration>? configure)` | Registers all ERA clients, applying the configure action to customize settings |
| `AddEraAdapter(EraClientConfiguration? config)` | Registers all ERA clients with the provided configuration (falls back to defaults if `null`) |

Example with configuration:

```csharp
builder.Services.AddEraAdapter(config =>
{
    config.AuthenticationUrl = "https://custom-auth-url";
    config.EraAsbestUrl = "https://custom-asbest-url";
});
```

---

## `EraClientConfiguration`

Configuration class for the ERA client, found in `Arbeidstilsynet.Common.EraClient.DependencyInjection`.

### Constructor

```csharp
public EraClientConfiguration(int timeoutInSeconds = 30, RetryStrategyOptions? retryStrategy = null)
```

- `timeoutInSeconds` — Timeout in seconds for the resilience pipeline (default: `30`).
- `retryStrategy` — Custom Polly `RetryStrategyOptions`. Defaults to exponential backoff with jitter, 2 s base delay, 5 max retries.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `AuthenticationUrl` | `string?` | Base URL for authentication. If not set, resolved from the host environment. |
| `EraAsbestUrl` | `string?` | Base URL for asbest endpoints. If not set, resolved from the host environment. |
| `ResiliencePipeline` | `ResiliencePipeline` | Polly resilience pipeline used for all API calls. Built from constructor parameters. |

---

## `EraEnvironment` Enum

Represents the target ERA environment. Found in `Arbeidstilsynet.Common.EraClient.Model`.

| Value | Description |
|-------|-------------|
| `Verifi` | Verification / test environment |
| `Valid` | Validation environment |
| `Prod` | Production environment |

---

## Extension Methods (`AltinnExtensions`)

Helper extensions in `Arbeidstilsynet.Common.EraClient.Extensions` for mapping host environments to ERA environments.

| Method | Signature | Description |
|--------|-----------|-------------|
| `GetRespectiveEraEnvironment` | `this IHostEnvironment, HttpContext → EraEnvironment` | Maps the current host environment to an `EraEnvironment`. Returns `Prod` in production; otherwise checks for the `"valid"` feature flag and returns `Valid` or `Verifi`. |
| `MapToString` | `this EraEnvironment → string` | Returns the lowercase string representation of the enum value. |
| `IsFeatureEnabled` | `this IHostEnvironment, string, HttpContext? → bool` | Checks if a feature flag is enabled by reading the `TEST_FLAGG` cookie. Always returns `false` in production. |

---

## Available Clients

| Interface | Purpose |
|-----------|---------|
| `IAuthenticationClient` | Obtain OAuth access tokens for ERA |
| `IEraAsbestClient` | Asbest (asbestos) domain operations |

*(Bemanning and Bilpleie clients are planned but not yet implemented.)*

---

## Usage Pattern

ERA clients require an authenticated token before making domain calls. Always authenticate first, then pass the result to domain methods.

### Authentication

```csharp
public class MyService(IAuthenticationClient authClient)
{
    private async Task<AuthenticationResponseDto> Authenticate()
    {
        return await authClient.Authenticate(new AuthenticationRequestDto
        {
            ClientId = "...",
            ClientSecret = "...",
        });
    }
}
```

### Asbest domain (`IEraAsbestClient`)

```csharp
public class AsbestService(IAuthenticationClient authClient, IEraAsbestClient asbestClient)
{
    public async Task Example(string orgNumber)
    {
        var auth = await authClient.Authenticate(new AuthenticationRequestDto { ... });

        // Get all asbest meldinger for an organisation
        List<Melding> meldinger = await asbestClient.GetMeldingerByOrg(auth, orgNumber);

        // Check status of an existing søknad
        SøknadStatusResponse? søknadStatus =
            await asbestClient.GetStatusForExistingSøknad(auth, orgNumber);

        // Get godkjenning status
        GodkjenningStatusResponse? godkjenning =
            await asbestClient.GetGodkjenningstatus(auth, orgNumber);

        // Get behandlings status
        BehandlingsstatusResponse? behandling =
            await asbestClient.GetBehandlingsstatus(auth, orgNumber);
    }
}
```

---

## Key Model Types

| Type | Description |
|------|-------------|
| `AuthenticationRequestDto` | OAuth client credentials request |
| `AuthenticationResponseDto` | OAuth token response (pass to domain clients) |
| `Melding` | Asbest melding record |
| `SøknadStatusResponse` | Status of an existing asbest søknad |
| `GodkjenningStatusResponse` | Godkjenning (approval) status |
| `BehandlingsstatusResponse` | Behandlings (processing) status |

---

## Notes

- The authentication token is passed to each domain method call; consider caching it for its lifetime to avoid excessive authentication calls.
- All `IEraAsbestClient` methods accept the `AuthenticationResponseDto` returned by `IAuthenticationClient.Authenticate`.
- This client targets the **external** ERA API — for internal ERA APIs (within Arbeidstilsynet's network), use the internal ERA integration instead.

---

## Adding ERA Integration — Checklist

1. `dotnet add package Arbeidstilsynet.Common.EraClient`
2. Contact `#team-godkjenning` for client credentials
3. Register with `services.AddEraAdapter()`
4. Inject `IAuthenticationClient` and the relevant domain client (`IEraAsbestClient`)
5. Authenticate first, then pass `AuthenticationResponseDto` to domain calls
6. Consider caching the token for its lifetime to avoid excessive authentication calls
