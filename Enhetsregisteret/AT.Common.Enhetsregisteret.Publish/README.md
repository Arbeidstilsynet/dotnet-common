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


### 1. Automatic URL selection based on environment

The client will use the correct Brreg API base URL depending on your ASP.NET Core environment:

- **Production** (`IWebHostEnvironment.IsProduction()`):  
  Uses `https://data.brreg.no/`
- **Non-production** (Development, Staging, etc.):  
  Uses `https://data.ppe.brreg.no/`

### 2. Overriding the API URL

You can override the base URL by setting `BrregApiBaseUrlOverwrite` in the configuration.

You can add the Enhetsregisteret client to your service collection in two ways:

### 3. Example: Registering the client in `Program.cs` (.NET 6+)

```csharp
using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnhetsregisteret(
    builder.Environment,
    config => {
        // Optional: override the API base URL
        // config.BrregApiBaseUrlOverwrite = "https://custom-url/";
        // Optional: disable caching
        // config.CacheOptions = new CacheOptions { Disabled = true };
    }
);

var app = builder.Build();

// more stuff...

app.Run();
```

### 4. Example: Registering in `Startup.cs` (older ASP.NET Core)

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddEnhetsregisteret(
        Environment, // IWebHostEnvironment
        config => {
            // config.BrregApiBaseUrlOverwrite = "https://custom-url/";
        }
    );
}
```

**Note:**  
- The client uses in-memory caching by default. You can disable it via `config.CacheOptions = new CacheOptions { Disabled = true };`.
- If you do not provide a configuration, the defaults (production or PPE URL, caching enabled)

---

## 🧑‍💻 Usage

`AddEnhetsregisteret(...)` registers two things you can inject:

- The generated `EnhetsregisteretClient` — the full Brreg API surface, exposed as fluent request builders.
- `Ports.IEnhetsregisteret` — a ready-made adapter over the client covering the most common operations (get/search enheter and underenheter, plus oppdateringshistorikk), returning the generated models. Pair it with the `EnhetsregisteretExtensions` helpers (e.g. `GetUnderenheterByHovedenhet`, `EnumerateEnheter`) for pagination-aware enumeration.

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