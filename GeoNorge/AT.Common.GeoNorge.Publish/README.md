# Introduction

This package provides an abstraction for the GeoNorge APIs [Adresser](https://ws.geonorge.no/adresser/v1/) and [Fylker og Kommuner](https://ws.geonorge.no/fylker-kommune/v1/), and some useful extension methods if you need to:

- find the closest address to a given coordinate
- find the location of a specific address
- get information about Norwegian counties (fylker) and municipalities (kommuner)

## 📖 Installation

To install GeoNorge, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.GeoNorge
```

## 🧑‍💻 Usage

Add to your service collection:

```csharp
public static IServiceCollection AddServices
    (
        this IServiceCollection services,
        DatabaseConfiguration databaseConfiguration
    ) {
        services.AddGeoNorge();
        return services;
    }
```

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
        var paginatedResult = await _addressSearch.SearchAddresses(new TextSearchQuery
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
