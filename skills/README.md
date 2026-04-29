# Agent Skills — Arbeidstilsynet/dotnet-common

Reusable [agent skills](https://agentskills.io) for the shared .NET libraries in this repository.

## Available Skills

| Skill | Description |
|-------|-------------|
| [altinn](altinn/) | Altinn 3 APIs — instances, events, correspondence, Maskinporten auth |
| [altinn-app](altinn-app/) | Utilities for building Altinn applications — data processors, country codes, extensions |
| [aspnetcore](aspnetcore/) | ASP.NET Core setup — middleware, OpenAPI, auth, CORS, OpenTelemetry, health checks |
| [enhetsregisteret](enhetsregisteret/) | Norwegian business registry (Brreg) client — company/organisation lookup |
| [era-client](era-client/) | ERA external API client with OAuth authentication |
| [feature-flags](feature-flags/) | Feature flag evaluation backed by Unleash |
| [geonorge](geonorge/) | Norwegian geographic data — address search, geocoding, county/municipality lookup |
| [test-extensions](test-extensions/) | Testing utilities — WireMock server setup from OpenAPI specifications |

## Installation

Install all skills into your project using the [skills CLI](https://github.com/vercel-labs/skills):

```bash
npx skills add Arbeidstilsynet/dotnet-common
```

Or install specific skills:

```bash
npx skills add Arbeidstilsynet/dotnet-common --skill altinn --skill aspnetcore
```
