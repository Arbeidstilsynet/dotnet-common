# Arbeidstilsynet.Common.GeoNorge

An abstraction over the GeoNorge APIs [Adresser](https://ws.geonorge.no/adresser/v1/) and [Kommuneinfo](https://api.kartverket.no/kommuneinfo/v1/). It ships:

- The high-level ports `IAddressSearch` and `IFylkeKommuneApi` (plus `AddressSearchExtensions`) that cover the most common use cases and return the [Kiota](https://learn.microsoft.com/openapi/kiota/)-generated models directly.
- The [Kiota](https://learn.microsoft.com/openapi/kiota/)-generated `AdresserClient` and `KommuneInfoClient` (generated from the official OpenAPI specifications) for advanced scenarios where you need the full API surface and want to adapt it locally.

Use the ports if you need to:

- find the closest address to a given coordinate
- find the location of a specific address
- get information about Norwegian counties (fylker) and municipalities (kommuner)

## 📖 Installation

```bash
dotnet add package Arbeidstilsynet.Common.GeoNorge
```

## 🧑‍💻 Registering the services

Add to your service collection:

```csharp
using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;

builder.Services.AddGeoNorge();
```

This registers the ports (`IAddressSearch`, `IFylkeKommuneApi`) as well as the generated clients (`AdresserClient`, `KommuneInfoClient`).

By default the clients target `https://ws.geonorge.no/` (Adresser under `/adresser/v1` and Kommuneinfo under `/kommuneinfo/v1`). You can override the base URL and opt into approximate Svalbard/Jan Mayen data:

```csharp
builder.Services.AddGeoNorge(
    new GeoNorgeConfig
    {
        BaseUrl = "https://ws.geonorge.no/",
        // Svalbard and Jan Mayen are not part of the GeoNorge dataset.
        // When enabled, IFylkeKommuneApi is decorated to supplement responses
        // with synthetic entries for Svalbard (fylke 21, kommune 2100) and
        // Jan Mayen (fylke 22, kommune 2211).
        UseApproximateSvalbardAndJanMayen = true,
    }
);
```

If you need to customize the retry/timeouts/circuit-breaker behavior, use the resilience callback:

```csharp
builder.Services.AddGeoNorge(
    new GeoNorgeConfig { BaseUrl = "https://ws.geonorge.no/" },
    options =>
    {
        options.Retry.MaxRetryAttempts = 2;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(1);
    }
);
```

## Usage

### Address Search Examples

```csharp
public class AddressService
{
    private readonly IAddressSearch _addressSearch;

    // Inject the IAddressSearch interface into your service
    public AddressService(IAddressSearch addressSearch)
    {
        _addressSearch = addressSearch;
    }

    public async Task Examples()
    {
        // Find the closest address to a given coordinate:
        var address = await _addressSearch.GetClosestAddress(new PointSearchQuery{
            Latitude = 59.9139,
            Longitude = 10.7522,
            RadiusInMeters = 1000
        });

        // If you want to find all addresses within a certain radius of a point:
        var paginatedResult = await _addressSearch.SearchAddressesByPoint(new PointSearchQuery
        {
            Latitude = 59.9139,
            Longitude = 10.7522,
            RadiusInMeters = 1000
        });

        // Search for addresses by a text query:
        var searchResult = await _addressSearch.SearchAddresses(new TextSearchQuery
        {
            SearchTerm = "Karl Johans gate 1, Oslo"
        });

        // If you only want the first result, you can use the QuickSearchLocation extension method:
        var location = await _addressSearch.QuickSearchLocation(new TextSearchQuery
        {
            SearchTerm = "Karl Johans gate 1, Oslo"
        });
    }
}
```

### County and Municipality Examples

```csharp
public class LocationService
{
    private readonly IFylkeKommuneApi _fylkeKommuneApi;

    // Inject the IFylkeKommuneApi interface into your service
    public LocationService(IFylkeKommuneApi fylkeKommuneApi)
    {
        _fylkeKommuneApi = fylkeKommuneApi;
    }

    public async Task Examples()
    {
        // Get all Norwegian counties (fylker):
        var fylker = await _fylkeKommuneApi.GetFylker();

        // Get all Norwegian municipalities (kommuner):
        var kommuner = await _fylkeKommuneApi.GetKommuner();

        // Get detailed information about all counties:
        var fylkerFullInfo = await _fylkeKommuneApi.GetFylkerFullInfo();

        // Get a specific county by number:
        var oslo = await _fylkeKommuneApi.GetFylkeByNumber("03");

        // Get detailed information about a specific municipality:
        var osloKommune = await _fylkeKommuneApi.GetKommuneByNumber("0301");

        // Find which municipality contains a specific point:
        var kommune = await _fylkeKommuneApi.GetKommuneByPoint(new PointQuery
        {
            Latitude = 59.9139,
            Longitude = 10.7522
        });
    }
}
```

## Advanced: using the generated clients directly

`AddGeoNorge(...)` also exposes the Kiota-generated `AdresserClient` and `KommuneInfoClient`. Use them when you need endpoints or fields that the ports do not surface. It is recommended that you wrap them in your own narrow interface that matches the intended use in that service or bounded context. That keeps tests simpler, avoids coupling every consumer to the full GeoNorge API surface, and gives you a stable seam if the generated fluent API changes later.

```csharp
using Arbeidstilsynet.Common.GeoNorge.Adresser;

public interface IAddressLookup
{
    Task<OutputGeoPoint?> GetClosestAddress(
        double latitude,
        double longitude,
        int radiusInMeters,
        CancellationToken cancellationToken
    );
}

public sealed class GeoNorgeAddressLookup(AdresserClient adresserClient) : IAddressLookup
{
    public async Task<OutputGeoPoint?> GetClosestAddress(
        double latitude,
        double longitude,
        int radiusInMeters,
        CancellationToken cancellationToken
    )
    {
        var result = await adresserClient.Punktsok.GetAsync(
            config =>
            {
                config.QueryParameters.Lat = (float)latitude;
                config.QueryParameters.Lon = (float)longitude;
                config.QueryParameters.Radius = radiusInMeters;
            },
            cancellationToken
        );

        return result?.Adresser?.FirstOrDefault();
    }
}
```

Register the adapter alongside the clients:

```csharp
builder.Services.AddGeoNorge();
builder.Services.AddScoped<IAddressLookup, GeoNorgeAddressLookup>();
```
