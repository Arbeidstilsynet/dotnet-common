# Enhetsregisteret Client

## 📚 Introduction

**Enhetsregisteret** is a .NET client library for accessing the Norwegian Register of Business Enterprises (Brreg).

The client and its request/response models are generated with [Kiota](https://learn.microsoft.com/openapi/kiota/) directly from Brreg's OpenAPI specification (`openapi.json`, shipped with the package). The package exposes the generated `EnhetsregisteretClient` together with dependency injection and environment-based configuration, so you can adapt the full Brreg API surface to your own needs.

---

## 📖 Installation

To install the Enhetsregisteret client, use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.Enhetsregisteret
```

---

## 🛠️ Configuration & Setup


### Environment-based registration

```csharp
using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnhetsregisteret(builder.Environment);

var app = builder.Build();

// more stuff...

app.Run();
```

| Host environment | Default target | Endpoint configuration |
| --- | --- | --- |
| Production | `https://data.brreg.no/` | Other targets and non-default URLs are rejected at registration |
| Staging | `https://data.ppe.brreg.no/` | Overrides are allowed, with a startup warning |
| Development | Offline `IEnhetsregisteret` stub | An explicit URL activates the real adapter and generated client |
| Other names (Test, QA, etc.) | No default | Set `EnhetsregisteretConfig.Environment` explicitly |

Available targets are `EnhetsregisteretEnvironment.Production`, `.Ppe` and `.Stub`. Set `Environment` to use a real API explicitly in Development, or to select a target for a custom host environment.

Both the configuration object and callback overloads are supported. Real clients use in-memory caching by default; disable it with `config.CacheOptions = new CacheOptions { Disabled = true };`.

### Development and docker-compose e2e tests

The Development stub runs in-process and makes no network requests. It registers `IEnhetsregisteret`, **not** the generated `EnhetsregisteretClient`. Direct injection of the generated client therefore fails at DI resolution unless you select a real target or supply an endpoint.

Current sample data:

- Enhet `111111111`, named `Eksempel AS`, with the Oslo business address `Eksempelveien 1`, municipality `0301`, postcode `0001` (Oslo).
- Underenhet `111111110`, named `Eksempel AS`, with the same sample location address.
- Lookups of other valid nine-digit organisation numbers return null; malformed numbers throw.
- Searches support `Organisasjonsnummer`, case-insensitive substring `Navn`, and `Page`/`Size`. Underenhet searches also support `OverordnetEnhet` (`111111111` for the current sample). Non-matches and later pages return empty lists.
- Update history is empty; `Page`, `Size`, `Dato` and `Oppdateringsid` are accepted. Pagination helpers terminate normally.

Other search filters throw `NotSupportedException`. Cancellation is honoured, and returned models are fresh per call. These are **simple samples, not a realistic dataset**: no coherence is guaranteed between responses or with GeoNorge. Consumers needing particular relationships or behaviours must replace the interface or configure their own endpoint.

To point Development at a mock server, with no custom adapter needed:

```csharp
builder.Services.AddEnhetsregisteret(
    builder.Environment,
    config => config.BrregApiBaseUrlOverwrite = "http://brreg-mock:8080/"
);
```

Overrides must be absolute HTTP(S) URLs without credentials, query strings or fragments. An explicit canonical URL also activates the real client in Development. Alternatively, register your own implementation after the package registration: `builder.Services.AddScoped<IEnhetsregisteret, MyEnhetsregisteret>()`.

### Migrating from 3.x

Development no longer contacts PPE by default. Set `Environment = EnhetsregisteretEnvironment.Ppe` to retain that behaviour, or supply a mock endpoint. Production no longer permits endpoint overrides, and custom host environment names require an explicit target.

---

## 🧑‍💻 Usage

When configured for a real API, `AddEnhetsregisteret(...)` registers two things you can inject:

- The generated `EnhetsregisteretClient` — the full Brreg API surface, exposed as fluent request builders.
- `Ports.IEnhetsregisteret` — a ready-made adapter over the client covering the most common operations (get/search enheter and underenheter, plus oppdateringshistorikk), returning the generated models. Pair it with the `EnhetsregisteretExtensions` helpers (e.g. `GetUnderenheterByHovedenhet`, `EnumerateEnheter`) for pagination-aware enumeration.

In default Development mode only `Ports.IEnhetsregisteret` is registered, backed by the built-in stub described above. Custom adapters over the generated client require a real target or explicit URL in Development.

### Recommended: wrap the client in your own adapter interface

The generated `EnhetsregisteretClient` exposes the _entire_ Brreg API. Depending on it directly couples your code to that large, generated surface. Instead, **define a small interface describing only the operations your service actually needs, and implement it as a thin adapter over the client.** This keeps the rest of your application decoupled from Brreg, makes the seam trivial to mock in tests, and lets you translate Brreg's transport-level behaviour (e.g. HTTP status codes) into domain semantics that fit your use case.

The example below exposes a single "look up an enhet by organisasjonsnummer" operation. A missing enhet surfaces from Brreg as an HTTP `404`, which Kiota raises as an `ApiException`; the adapter catches it and returns `null` so callers deal with a simple nullable result instead of exception handling:

```csharp
using Arbeidstilsynet.Common.Enhetsregisteret;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Microsoft.Kiota.Abstractions;

// Minimal, use-case-specific interface — the only Brreg surface the rest of your app sees.
public interface IEnhetLookup
{
    Task<Enhet?> GetEnhet(string organisasjonsnummer, CancellationToken cancellationToken);
}

public sealed class EnhetLookup(EnhetsregisteretClient client) : IEnhetLookup
{
    public async Task<Enhet?> GetEnhet(
        string organisasjonsnummer,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await client
                .Enhetsregisteret
                .Api
                .Enheter[organisasjonsnummer]
                .GetAsync(cancellationToken: cancellationToken);

            return response?.Enhet;
        }
        catch (ApiException ex) when (ex.ResponseStatusCode == 404)
        {
            // Unknown organisasjonsnummer — treat "not found" as an empty result.
            return null;
        }
    }
}
```

Register the adapter alongside the client:

```csharp
builder.Services.AddEnhetsregisteret(builder.Environment);
builder.Services.AddScoped<IEnhetLookup, EnhetLookup>();
```

The rest of your application then depends on `IEnhetLookup` — not on `EnhetsregisteretClient` — so it only ever sees the narrow surface you chose to expose.

> **Note:** `Ports.IEnhetsregisteret` is registered and implemented out of the box as a thin adapter over the generated client. Use it (with `EnhetsregisteretExtensions`) for common operations, or write your own adapter as shown above when you want an even narrower, domain-specific seam.

### Injecting the generated client directly

If you prefer, you can inject `EnhetsregisteretClient` and use its fluent request builders directly. Prefer the adapter pattern above for anything beyond quick prototypes:

```csharp
using Arbeidstilsynet.Common.Enhetsregisteret;

public class MyService(EnhetsregisteretClient client)
{
    public async Task<string?> GetNavn(
        string organisasjonsnummer,
        CancellationToken cancellationToken
    )
    {
        var response = await client
            .Enhetsregisteret
            .Api
            .Enheter[organisasjonsnummer]
            .GetAsync(cancellationToken: cancellationToken);

        return response?.Enhet?.Navn;
    }
}
```

---

## 📝 Notes

- All API methods are asynchronous.
- Requests and responses use the Kiota-generated models under `Arbeidstilsynet.Common.Enhetsregisteret.Models`.
- The client uses in-memory caching by default. You can disable it via `config.CacheOptions = new CacheOptions { Disabled = true };`.
- For the full API surface, explore the fluent request builders on `EnhetsregisteretClient` (e.g. `.Enhetsregisteret`, `.Frivillighetsregisteret`, `.Partiregisteret`).

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!  
Feel free to open an issue or submit a pull request.

---

## 📄 License

This project is licensed under the MIT License.