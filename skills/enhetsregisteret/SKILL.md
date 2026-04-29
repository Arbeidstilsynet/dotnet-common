---
name: enhetsregisteret
description: Norwegian business registry (Brreg) client for looking up companies and organisations using AT.Common.Enhetsregisteret. Use this skill when looking up enheter or underenheter by organisation number, searching for companies, or fetching update history from the Norwegian Register of Business Enterprises.
license: MIT
metadata:
  domain: backend
  tags: enhetsregisteret brreg norway business-registry organisation company dotnet client
---

# Enhetsregisteret Skill — Arbeidstilsynet/dotnet-common

`Arbeidstilsynet.Common.Enhetsregisteret` (`AT.Common.Enhetsregisteret.Publish`) is a strongly-typed .NET client for the Norwegian Register of Business Enterprises (Brreg).

---

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.Enhetsregisteret
```

---

## Dependency Injection Setup

```csharp
using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;

builder.Services.AddEnhetsregisteret(
    builder.Environment,
    config =>
    {
        // Optional: override the Brreg API base URL
        // config.BrregApiBaseUrlOverwrite = "https://custom-url/";

        // Optional: disable in-memory caching
        // config.CacheOptions = new CacheOptions { Disabled = true };
    }
);
```

### Automatic URL selection

| Environment | API URL used |
|-------------|-------------|
| Production | `https://data.brreg.no/` |
| Non-production (dev, staging, …) | `https://data.ppe.brreg.no/` |

Caching is enabled by default. Override with `config.BrregApiBaseUrlOverwrite` or disable with `config.CacheOptions = new CacheOptions { Disabled = true }`.

---

## `IEnhetsregisteret` Interface

Inject `IEnhetsregisteret` into any service or controller.

```csharp
public class MyService(IEnhetsregisteret enhetsregisteret) { }
```

### Look up a single unit

```csharp
// Main unit (enhet)
Enhet? enhet = await enhetsregisteret.GetEnhet("123456789");

// Sub-unit (underenhet)
Underenhet? underenhet = await enhetsregisteret.GetUnderenhet("987654321");
```

Both return `null` if the unit is not found or an error occurs.

### Search

```csharp
var searchParams = new SearchEnheterQuery
{
    Navn = "Arbeidstilsynet",
    Kommunenummer = "5001",
};

var pagination = new Pagination { Side = 0, Størrelse = 20 };

// Search main units
PaginationResult<Enhet>? enheter = await enhetsregisteret.SearchEnheter(searchParams, pagination);

// Search sub-units
PaginationResult<Underenhet>? underenheter = await enhetsregisteret.SearchUnderenheter(searchParams, pagination);
```

Returns `null` on error.

### Update history

```csharp
var query = new GetOppdateringerQuery
{
    Dato = DateOnly.FromDateTime(DateTime.Today.AddDays(-7)),
};

// Updates for main units
PaginationResult<Oppdatering>? enheterOps =
    await enhetsregisteret.GetOppdateringerEnheter(query, pagination);

// Updates for sub-units
PaginationResult<Oppdatering>? underenheterOps =
    await enhetsregisteret.GetOppdateringerUnderenheter(query, pagination);
```

---

## Key Model Types

| Type | Description |
|------|-------------|
| `Enhet` | Main business unit with name, organisation number, address, NACE codes, etc. |
| `Underenhet` | Sub-unit linked to a parent `Enhet` |
| `Oppdatering` | Record of a change to an `Enhet` or `Underenhet` |
| `SearchEnheterQuery` | Search filter (name, municipality, industry code, …) |
| `GetOppdateringerQuery` | Filter for update history (date range) |
| `Pagination` | `Side` (page index) + `Størrelse` (page size) |
| `PaginationResult<T>` | `Data`, `TotalElements`, `TotalPages` |

---

## Validation Utilities

The package includes `Enhetsregisteret.Validation` helpers for validating Norwegian organisation numbers:

```csharp
// Check if a string is a valid 9-digit Norwegian org number
bool valid = OrganisasjonsnummerValidator.IsValid("123456789");
```

---

## Notes

- All methods are `async` and return `null` (not throw) on network errors or 404 responses.
- In-memory caching reduces repeated round-trips to Brreg for the same org number.
- For XML documentation on available search fields, see the source of `SearchEnheterQuery`.

---

## Adding Enhetsregisteret — Checklist

1. `dotnet add package Arbeidstilsynet.Common.Enhetsregisteret`
2. Register with `services.AddEnhetsregisteret(environment)`
3. Inject `IEnhetsregisteret` into your service
4. Handle `null` returns (not-found / error)
5. Use `OrganisasjonsnummerValidator.IsValid` to validate input before calling the API
