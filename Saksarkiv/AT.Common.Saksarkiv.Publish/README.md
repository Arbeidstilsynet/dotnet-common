# Arbeidstilsynet.Common.Saksarkiv

Generated Saksarkiv client with reusable configuration and dependency injection extensions.

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.Saksarkiv
```

## What the package contains

- The generated `SaksarkivClientV3` (API **v3**, the default) and request/response models
- The generated `SaksarkivClient` (legacy API **v2**), available on demand
- `AddSaksarkivClient(...)` for dependency injection (registers the **v3** client)
- `AddSaksarkivClientV2(...)` to additionally register the legacy **v2** client
- `SaksarkivConfiguration` for base URL and scope
- `ISaksarkivTokenProvider` so the consuming app can decide how access tokens are fetched

## Registering the client

The package does not depend on Texas or any other project-specific token provider. Consumers must register an `ISaksarkivTokenProvider` implementation themselves.

`AddSaksarkivClient(...)` registers the **v3** client (`SaksarkivClientV3`) as the default.

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

### Using the legacy v2 client on demand

If you still need the legacy v2 API, additionally call `AddSaksarkivClientV2(...)` and resolve
`SaksarkivClient`. Both registrations share the same HTTP client, resilience, authentication, and
configuration.

```csharp
builder.Services.AddSaksarkivClient(configuration);   // v3 (default)
builder.Services.AddSaksarkivClientV2(configuration);  // v2 (on demand)
```

> Note: the `Saksarkiv` health check is only registered by `AddSaksarkivClient(...)` (v3).

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
- retries disabled for all `POST` requests, for the v3 health probe `/api/v3/metadata/tilgangskoder`, and for `/apiv2/health*`

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

You can use `SaksarkivClientV3` directly, but it is highly recommended that you wrap it in your own narrow interface that matches the intended use in that service or bounded context. That keeps tests simpler, avoids coupling every consumer to the full Saksarkiv API surface, and gives you a stable seam if the generated fluent API changes later.

Define an interface for only the operations your service needs, and implement it as a thin adapter over `SaksarkivClientV3`:

```csharp
using Arbeidstilsynet.Common.Saksarkiv.V3;
using Arbeidstilsynet.Common.Saksarkiv.V3.Models.AT.EArkiv.Entiteter.API.V3;

public interface IArchiveClient
{
    Task<List<Mappetype>?> GetCaseTypes(CancellationToken cancellationToken);
}

public sealed class SaksarkivArchiveClient(SaksarkivClientV3 saksarkivClient) : IArchiveClient
{
    public async Task<List<Mappetype>?> GetCaseTypes(CancellationToken cancellationToken)
    {
        return await saksarkivClient.Api.V3.Metadata.Sakstyper.GetAsync(
            cancellationToken: cancellationToken
        );
    }
}
```

Register the adapter alongside the client:

```csharp
builder.Services.AddScoped<ISaksarkivTokenProvider, MySaksarkivTokenProvider>();
builder.Services.AddSaksarkivClient(...);
builder.Services.AddScoped<IArchiveClient, SaksarkivArchiveClient>();
```

The rest of your application then depends on `IArchiveClient`, not on `SaksarkivClientV3` directly.

`AddSaksarkivClient(...)` also registers a health check named `Saksarkiv`.

## Creating a case (opprett sak)

The v3 spec models the create-case payload as a query parameter, so the generated
`Api.V3.Saker.PostAsync(...)` takes an untyped `MultipartBody`. The package ships a typed
`OpprettSakAsync(...)` extension that wraps this: it serializes the generated `OpprettSakRequest`
model into the JSON `payload` part and attaches any files.

```csharp
using Arbeidstilsynet.Common.Saksarkiv.V3;
using Arbeidstilsynet.Common.Saksarkiv.V3.Models.AT.EArkiv.Entiteter.API.V3;

public sealed class ArchiveService(SaksarkivClientV3 client)
{
    public async Task<string?> CreateCase(CancellationToken ct)
    {
        var request = new OpprettSakRequest
        {
            Tittel = "Min sak",
            Saksbehandler = "user@arbeidstilsynet.no",
            AnsvarligEnhetKode = "ABC",
            EksternId = Guid.NewGuid().ToString(), // client-generated, required
            Sakstype = "...",
            Arkivkode = "...",
            Tilgangskode = "...",
            Journalposter =
            [
                new OpprettSakRequestJournalpost
                {
                    EksternId = Guid.NewGuid().ToString(),
                    JournalpostType = Journalposttype.Inngaaende,
                    Tittel = "Brev",
                    HoveddokumentFilnavn = "hoved.pdf",
                    PersonInfo = new OpprettSakRequestPersonInfo { Navn = "Ola Nordmann" },
                },
            ],
        };

        await using var pdf = File.OpenRead("hoved.pdf");
        var files = new[]
        {
            new SaksarkivFile
            {
                FileName = "hoved.pdf", // must match HoveddokumentFilnavn/VedleggFilnavn
                Content = pdf,
                ContentType = "application/pdf",
            },
        };

        // Asynchronous (202): returns a queued message you can track via Api.V3.Meldinger[id].
        var status = await client.OpprettSakAsync(request, files, cancellationToken: ct);
        return status?.MeldingId;
    }
}
```

File names in `SaksarkivFile.FileName` must match those referenced by `HoveddokumentFilnavn` /
`VedleggFilnavn` and be unique within the request.
