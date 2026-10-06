# Arbeidstilsynet.Common.GeoNorge

An abstraction over the GeoNorge APIs [Adresser](https://ws.geonorge.no/adresser/v1/), [Kommuneinfo](https://api.kartverket.no/kommuneinfo/v1/) and Kartverket's [Eiendom](https://api.kartverket.no/eiendom/v1/). It ships:

- The high-level ports `IAddressSearch`, `IFylkeKommuneApi` and `IPropertySearch` (plus `AddressSearchExtensions`) that cover the most common use cases and return the [Kiota](https://learn.microsoft.com/openapi/kiota/)-generated models directly.
- The [Kiota](https://learn.microsoft.com/openapi/kiota/)-generated `AdresserClient`, `KommuneInfoClient` and `EiendomClient` (generated from the official OpenAPI specifications) for local adaptation. `EiendomClient` includes only cadastral-number geocoding (`GET /geokoding`), not nearby-property searches.

Use the ports if you need to:

- find the closest address to a given coordinate
- find the location of a specific address
- find cadastral parcels and their locations, even when a property has no registered addresses
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

This registers the ports (`IAddressSearch`, `IFylkeKommuneApi`, `IPropertySearch`) as well as the generated clients (`AdresserClient`, `KommuneInfoClient`, `EiendomClient`).

By default Adresser and Kommuneinfo target `https://ws.geonorge.no/` (under `/adresser/v1` and `/kommuneinfo/v1`). Eiendom independently targets `https://api.kartverket.no/eiendom/v1` and requires no authentication. `BaseUrl` controls only the first two clients; `PropertyBaseUrl` controls Eiendom. Both settings take a host/base URL, with the API-specific path appended automatically. You can override them and opt into approximate Svalbard/Jan Mayen data:

```csharp
builder.Services.AddGeoNorge(
    new GeoNorgeConfig
    {
        BaseUrl = "https://ws.geonorge.no/",
        PropertyBaseUrl = "https://api.kartverket.no/",
        // Svalbard and Jan Mayen are not part of the GeoNorge dataset.
        // When enabled, IFylkeKommuneApi is decorated to supplement responses
        // with synthetic entries for Svalbard (fylke 21, kommune 2100) and
        // Jan Mayen (fylke 22, kommune 2211).
        UseApproximateSvalbardAndJanMayen = true,
    }
);
```

If you need to customize the retry/timeouts/circuit-breaker behavior, use the resilience callback (applied to all three clients):

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
using Arbeidstilsynet.Common.GeoNorge.Adresser.Punktsok;
using Arbeidstilsynet.Common.GeoNorge.Adresser.Sok;

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
        var address = await _addressSearch.GetClosestAddress(
            new PunktsokRequestBuilder.PunktsokRequestBuilderGetQueryParameters
            {
                Lat = 59.9139f,
                Lon = 10.7522f,
                Radius = 1000,
            }
        );

        // If you want to find all addresses within a certain radius of a point:
        var paginatedResult = await _addressSearch.SearchAddressesByPoint(
            new PunktsokRequestBuilder.PunktsokRequestBuilderGetQueryParameters
            {
                Lat = 59.9139f,
                Lon = 10.7522f,
                Radius = 1000,
            }
        );

        // Search for addresses by a text query:
        var searchResult = await _addressSearch.SearchAddresses(
            new SokRequestBuilder.SokRequestBuilderGetQueryParameters
            {
                Sok = "Karl Johans gate 1, Oslo",
            }
        );

        // If you only want the first result, you can use the QuickSearchLocation extension method:
        var location = await _addressSearch.QuickSearchLocation(
            new SokRequestBuilder.SokRequestBuilderGetQueryParameters
            {
                Sok = "Karl Johans gate 1, Oslo",
            }
        );
    }
}
```

### Cadastral Property Search

`IPropertySearch` is separate from `IAddressSearch`: it locates cadastral parcels, does not fabricate an address, and does not automatically fall back from address search.

```csharp
using Arbeidstilsynet.Common.GeoNorge.Eiendom.Geokoding;
using Arbeidstilsynet.Common.GeoNorge.Eiendom.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;

public class PropertyLocationService(IPropertySearch propertySearch)
{
    public Task<GeoKodingRespons?> FindParcels() =>
        propertySearch.SearchProperties(
            new GeokodingRequestBuilder.GeokodingRequestBuilderGetQueryParameters
            {
                Kommunenummer = "5512",
                Gardsnummer = 2,
                Bruksnummer = 16,
            }
        );
}
```

Municipality numbers are four-character strings, including leading zeros (for example `"0301"`). Optional `Festenummer` and `Seksjonsnummer` identify leaseholds/sections. Alternatively, set `Matrikkelnummer`, for example `"0301-223/60/0/3"`, instead of the individual identifier fields.

The port defaults `Utkoordsys` to **EPSG:4326 (WGS84)** when omitted; explicit values such as 4258 or 25833 are preserved. Generated clients used directly retain the upstream default of 4258 when the query omits the coordinate system. In WGS84, GeoJSON Point coordinates are **[longitude, latitude]**, not latitude first.

A successful response is a generated `GeoKodingRespons` with all matching `Features`. An empty `Features` collection means no matches. Network (`HttpRequestException`) and API (`ApiException`) failures are logged as warnings and return `null`, not an empty collection; unexpected exceptions propagate. Consumers should handle failure separately from a successful no-match response.

Each feature exposes typed cadastral `Properties`, including the generated `Hovedområde` main-parcel flag, and `Geometry.Type` / `Geometry.Coordinates`. The official OpenAPI leaves coordinate array items unspecified, so Kiota represents `Coordinates` as `UntypedNode` (arrays as `UntypedArray` with nested nodes). Inspect the geometry type before reading coordinates; do not assume every geometry is a point. Setting `Omrade = true` requests area geometries **instead of** representative points and retains their nested GeoJSON coordinates. An explicitly requested nonstandard coordinate system may include `Crs` in the response.

A property may have several parcels. For example, the addressless property `5512/2/16` returned two representative points on 2026-10-05: `[16.82662, 68.68422]` (main parcel) and `[16.82759, 68.68239]` (other parcel). The library neither selects the first/main parcel nor treats a representative point as the exact incident location. Parcel selection, address-empty fallback policy and map/location confirmation belong in consumers.

Kartverket's cadastral data normally lags Matrikkelen by one day; cadastral maps can be incomplete or imprecise. Area geometries do not include boundary-quality information. This API cannot guarantee a precise incident location.

### County and Municipality Examples

```csharp
using Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Punkt;

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
        var kommune = await _fylkeKommuneApi.GetKommuneByPoint(
            new PunktRequestBuilder.PunktRequestBuilderGetQueryParameters
            {
                Nord = 59.9139,
                Ost = 10.7522,
                Koordsys = 4326,
            }
        );
    }
}
```

## Advanced: using the generated clients directly

`AddGeoNorge(...)` also exposes the Kiota-generated `AdresserClient`, `KommuneInfoClient` and `EiendomClient`. Use them when you need fields or request configuration that the ports do not surface. Eiendom is restricted to `/geokoding`. It is recommended that you wrap them in your own narrow interface that matches the intended use in that service or bounded context. That keeps tests simpler, avoids coupling every consumer to the full GeoNorge API surface, and gives you a stable seam if the generated fluent API changes later.

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

## Regenerating the property client

The checked-in `openapi-eiendom.json` is a snapshot of the [official specification](https://api.kartverket.no/eiendom/v1/openapi.json) referenced by Swagger. Refresh it deliberately, then regenerate from the GeoNorge directory with the repository-pinned tools:

```powershell
dotnet tool restore
npm run generate:client:eiendom
```

The script generates only `GET /geokoding` into `Generated\Eiendom`, including `kiota-lock.json`. Do not edit generated files manually. Property tests use deterministic mock HTTP responses; live Kartverket checks are optional and not required by CI.
