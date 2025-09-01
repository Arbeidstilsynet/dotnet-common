# Enhetsregisteret Client

## 📚 Introduction

**Enhetsregisteret** is a .NET client library for accessing the Norwegian Register of Business Enterprises (Brreg).  
It provides a strongly-typed, easy-to-use API for querying company and organization data from official sources, enabling you to integrate business registry lookups into your .NET applications.

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

### Inject into your class

```csharp
public class MyService
{
    private readonly IEnhetsregisteret _enhetsregisteret;

    private readonly ILogger<MyService> _logger;
    public MyService(IEnhetsregisteret enhetsregisteret, ILogger<MyService> logger)
    {
        _enhetsregisteret = enhetsregisteret;
        _logger = logger;
    }

    public async Task GetEnhetExample()
    {
        var enhet = await _enhetsregisteret.GetEnhet("123456789");
        if (enhet != null)
        {
            _logger.LogInformation("Navn: {OrgNavn}, Organisasjonsnummer: {OrgNr}", enhet.Navn, enhet.Organisasjonsnummer);
        }
    }
}
```

---

## 🚀 API Overview

### `Task<Underenhet?> GetUnderenhet(string organisasjonsnummer)`
Get a single underenhet (sub-unit) by organization number.  
Returns `null` if not found or on error.

---

### `Task<Enhet?> GetEnhet(string organisasjonsnummer)`
Get a single enhet (main unit) by organization number.  
Returns `null` if not found or on error.

---

### `Task<PaginationResult<Underenhet>?> SearchUnderenheter(SearchEnheterQuery searchParameters, Pagination pagination)`
Search for underenheter (sub-units) using search parameters and pagination.  
Returns a paginated result of matching underenheter, or `null` on error.

---

### `Task<PaginationResult<Enhet>?> SearchEnheter(SearchEnheterQuery searchParameters, Pagination pagination)`
Search for enheter (main units) using search parameters and pagination.  
Returns a paginated result of matching enheter, or `null` on error.

---

### `Task<PaginationResult<Oppdatering>?> GetOppdateringerUnderenheter(GetOppdateringerQuery query, Pagination pagination)`
Get update history for underenheter (sub-units) with optional filtering and pagination.  
Returns a paginated result of updates, or `null` on error.

---

### `Task<PaginationResult<Oppdatering>?> GetOppdateringerEnheter(GetOppdateringerQuery query, Pagination pagination)`
Get update history for enheter (main units) with optional filtering and pagination.  
Returns a paginated result of updates, or `null` on error.

---

## 📝 Notes

- All API methods are asynchronous.
- Handle `null` results and exceptions as appropriate.
- For advanced queries or filtering, see the XML documentation in the code.

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!  
Feel free to open an issue or submit a pull request.

---

## 📄 License

This project is licensed under the MIT License.