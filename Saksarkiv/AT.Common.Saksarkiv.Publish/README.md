# Arbeidstilsynet.Common.Saksarkiv

Generated Saksarkiv client with reusable configuration and dependency injection extensions.

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.Saksarkiv
```

## What the package contains

- The generated `SaksarkivClient` and request/response models
- `AddSaksarkivClient(...)` for dependency injection
- `SaksarkivConfiguration` for base URL and scope
- `ISaksarkivTokenProvider` so the consuming app can decide how access tokens are fetched

## Registering the client

The package does not depend on Texas or any other project-specific token provider. Consumers must register an `ISaksarkivTokenProvider` implementation themselves.

```csharp
using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection;
using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection.Configuration;
using Arbeidstilsynet.Common.Saksarkiv.Ports;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISaksarkivTokenProvider, MySaksarkivTokenProvider>();

builder.Services.AddSaksarkivClient(
    new SaksarkivConfiguration
    {
        BaseUrl = "https://saksarkiv.example.no/",
        Scope = "api://saksarkiv/.default",
    }
);
```

If you need to customize the retry/timeouts/circuit-breaker behavior, use the resilience callback:

```csharp
builder.Services.AddScoped<ISaksarkivTokenProvider, MySaksarkivTokenProvider>();

builder.Services.AddSaksarkivClient(
    new SaksarkivConfiguration
    {
        BaseUrl = "https://saksarkiv.example.no/",
        Scope = "api://saksarkiv/.default",
    },
    options =>
    {
        options.Retry.MaxRetryAttempts = 2;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(1);
    }
);
```

The default resilience settings are:

- 2 retry attempts
- 5 seconds retry delay
- 15 seconds per-attempt timeout
- 1 minute total request timeout
- retries disabled for all `POST` requests and for `/apiv2/health*`

## Token provider example

```csharp
using Arbeidstilsynet.Common.Saksarkiv.Ports;

public sealed class MySaksarkivTokenProvider : ISaksarkivTokenProvider
{
    public async Task<string> GetAccessToken(
        string scope,
        CancellationToken cancellationToken = default
    )
    {
        // Replace with the token source used by your application.
        return await Task.FromResult("access-token");
    }
}
```

## Usage

You can use `SaksarkivClient` directly, but it is highly recommended that you wrap it in your own narrow interface that matches the intended use in that service or bounded context. That keeps tests simpler, avoids coupling every consumer to the full Saksarkiv API surface, and gives you a stable seam if the generated fluent API changes later.

Define an interface for only the operations your service needs, and implement it as a thin adapter over `SaksarkivClient`:

```csharp
using Arbeidstilsynet.Common.Saksarkiv;
using Arbeidstilsynet.Common.Saksarkiv.Models.Entiteter.Meldinger;

public interface IArchiveClient
{
    Task<bool?> CreateCase(OpprettSakParameter parameter, CancellationToken cancellationToken);
}

public sealed class SaksarkivArchiveClient(SaksarkivClient saksarkivClient) : IArchiveClient
{
    public async Task<bool?> CreateCase(OpprettSakParameter parameter, CancellationToken cancellationToken)
    {
        var response = await saksarkivClient.Apiv2.Sak.Opprett.PostAsync(
            parameter,
            cancellationToken: cancellationToken
        );

        return response?.Success;
    }
}
```

Register the adapter alongside the client:

```csharp
builder.Services.AddScoped<ISaksarkivTokenProvider, MySaksarkivTokenProvider>();
builder.Services.AddSaksarkivClient(...);
builder.Services.AddScoped<IArchiveClient, SaksarkivArchiveClient>();
```

The rest of your application then depends on `IArchiveClient`, not on `SaksarkivClient` directly.

`AddSaksarkivClient(...)` also registers a health check named `Saksarkiv`.
