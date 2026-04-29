---
name: geonorge
description: Norwegian geographic data — address search, reverse geocoding, county (fylke) and municipality (kommune) lookup using AT.Common.GeoNorge. Use this skill when finding an address from a text query, finding the closest address to coordinates, resolving Norwegian counties/municipalities, or finding which municipality contains a geographic point.
license: MIT
metadata:
  domain: backend
  tags: geonorge geography address geocoding coordinates fylke kommune norway dotnet
---

# GeoNorge Skill — Arbeidstilsynet/dotnet-common

`Arbeidstilsynet.Common.GeoNorge` (`AT.Common.GeoNorge.Publish`) wraps the GeoNorge [Adresser API](https://ws.geonorge.no/adresser/v1/) and [Fylker og Kommuner API](https://ws.geonorge.no/fylker-kommune/v1/) in strongly-typed .NET clients.

---

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.GeoNorge
```

---

## Dependency Injection Setup

```csharp
builder.Services.AddGeoNorge();
```

This registers `IAddressSearch` and `IFylkeKommuneApi`.

---

## Address Search (`IAddressSearch`)

### Text search

Find addresses matching a free-text query:

```csharp
public class AddressService(IAddressSearch addressSearch)
{
    public async Task Example()
    {
        var result = await addressSearch.SearchAddresses(new TextSearchQuery
        {
            SearchTerm = "Karl Johans gate 1, Oslo"
        });
        // result?.Data contains matching Address records

        // Get only the first match as a Location (lat/lon)
        var location = await addressSearch.QuickSearchLocation(new TextSearchQuery
        {
            SearchTerm = "Karl Johans gate 1, Oslo"
        });
    }
}
```

### Point search (reverse geocoding)

Find addresses within a radius of given coordinates:

```csharp
// Find all addresses within 1 000 m of a point
var result = await addressSearch.SearchAddressesByPoint(new PointSearchQuery
{
    Latitude = 59.9139,
    Longitude = 10.7522,
    RadiusInMeters = 1000
});

// Find the single closest address
var closest = await addressSearch.GetClosestAddress(new PointSearchQuery
{
    Latitude = 59.9139,
    Longitude = 10.7522,
    RadiusInMeters = 1000
});
```

### Pagination

Both search methods accept an optional `Pagination` parameter:

```csharp
var result = await addressSearch.SearchAddresses(
    new TextSearchQuery { SearchTerm = "Brannfjordveien" },
    new Pagination { PageIndex = 0, PageSize = 10 }
);
```

---

## County and Municipality (`IFylkeKommuneApi`)

```csharp
public class LocationService(IFylkeKommuneApi fylkeKommuneApi)
{
    public async Task Example()
    {
        // Get all counties
        var fylker = await fylkeKommuneApi.GetFylker();

        // Get all municipalities
        var kommuner = await fylkeKommuneApi.GetKommuner();

        // Get detailed county info (includes municipalities)
        var fylkerFull = await fylkeKommuneApi.GetFylkerFullInfo();

        // Get a specific county by number
        var oslo = await fylkeKommuneApi.GetFylkeByNumber("03");

        // Get a specific municipality by number
        var osloKommune = await fylkeKommuneApi.GetKommuneByNumber("0301");

        // Reverse-lookup: which municipality contains this point?
        var kommune = await fylkeKommuneApi.GetKommuneByPoint(new PointQuery
        {
            Latitude = 59.9139,
            Longitude = 10.7522
        });
    }
}
```

---

## Key Model Types

| Type | Description |
|------|-------------|
| `TextSearchQuery` | Free-text address search parameters |
| `PointSearchQuery` | Coordinates + radius for reverse geocoding |
| `PointQuery` | Coordinates for fylke/kommune lookup |
| `Address` | Full address record (street, number, municipality, coordinates) |
| `Location` | Latitude + Longitude point |
| `Pagination` | `Side` (page index) + `Størrelse` (page size) |
| `PaginationResult<T>` | `Data` + paging metadata |
| `Fylke` | Norwegian county |
| `Kommune` | Norwegian municipality |

---

## Extension Methods

`QuickSearchLocation` is a convenience method on `IAddressSearch` that returns the `Location` (lat/lon) of the first search result:

```csharp
Location? location = await addressSearch.QuickSearchLocation(
    new TextSearchQuery { SearchTerm = "Youngstorget, Oslo" }
);
```

Returns `null` if no address is found.

---

## Adding GeoNorge — Checklist

1. `dotnet add package Arbeidstilsynet.Common.GeoNorge`
2. Register with `services.AddGeoNorge()`
3. Inject `IAddressSearch` for address/geocoding operations
4. Inject `IFylkeKommuneApi` for county/municipality operations
5. Handle `null` returns (network errors or no results)
