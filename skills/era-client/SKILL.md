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
var appSettings = builder.Configuration.GetRequired<AppSettings>();

builder.Services.AddEraAdapter();
```

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

- The authentication token must be obtained before each domain request (or cached and refreshed when expired).
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
