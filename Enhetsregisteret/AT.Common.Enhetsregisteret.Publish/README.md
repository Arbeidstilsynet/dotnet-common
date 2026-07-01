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

`AddEnhetsregisteret(...)` registers the generated `EnhetsregisteretClient` in the service collection. Inject it directly, or — recommended — wrap it in your own narrow interface that matches the intended use in your service or bounded context. That keeps tests simpler, avoids coupling every consumer to the full Brreg API surface, and gives you a stable seam if the generated fluent API changes later.

### Inject the generated client

```csharp
using Arbeidstilsynet.Common.Enhetsregisteret;

public class MyService
{
    private readonly EnhetsregisteretClient _client;
    private readonly ILogger<MyService> _logger;

    public MyService(EnhetsregisteretClient client, ILogger<MyService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task GetEnhetExample(CancellationToken cancellationToken)
    {
        var enhet = await _client
            .Enhetsregisteret
            .Api
            .Enheter["123456789"]
            .GetAsync(cancellationToken: cancellationToken);

        if (enhet != null)
        {
            _logger.LogInformation(
                "Navn: {OrgNavn}, Organisasjonsnummer: {OrgNr}",
                enhet.Navn,
                enhet.Organisasjonsnummer
            );
        }
    }
}
```

### Wrap the client in your own interface (recommended)

Define an interface for only the operations your service needs, and implement it as a thin adapter over `EnhetsregisteretClient`:

```csharp
using Arbeidstilsynet.Common.Enhetsregisteret;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;

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
        return await client
            .Enhetsregisteret
            .Api
            .Enheter[organisasjonsnummer]
            .GetAsync(cancellationToken: cancellationToken);
    }
}
```

Register the adapter alongside the client:

```csharp
builder.Services.AddEnhetsregisteret(builder.Environment);
builder.Services.AddScoped<IEnhetLookup, EnhetLookup>();
```

The rest of your application then depends on `IEnhetLookup`, not on `EnhetsregisteretClient` directly.

> **Note:** `Ports.IEnhetsregisteret` and the `EnhetsregisteretExtensions` helper methods are still present in the package but are **not** registered or implemented out of the box. They are kept as a starting point for building your own adapter over the generated client.

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