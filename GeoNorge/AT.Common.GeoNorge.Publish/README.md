# Introduction

This package provides an abstraction for the [GeoNorge API](https://ws.geonorge.no/adresser/v1/), and some useful extension methods if you need to:

- find the closest address to a given coordinate
- find the location of a specific address.

## 📖 Installation

To install the GeoNorge, you can use the following command in your terminal:

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

Code examples:

```csharp
public class MyService
{
    private readonly IGeoNorge _geoNorge;

    // Inject the IGeoNorge interface into your service
    public MyService(IGeoNorge geoNorge)
    {
        _geoNorge = geoNorge;
    }

    public async Task Examples()
    {
        // Find the closest address to a given coordinate:
        var address = await _geoNorge.GetClosestAddress(new PointSearchQuery{
            Latitude = 59.9139,
            Longitude = 10.7522,
            RadiusInMeters = 1000
        });

        // If you want to find all addresses within a certain radius of a point:
        var paginatedResult = await _geoNorge.SearchAddressesByPoint(new PointSearchQuery
        {
            Latitude = 59.9139,
            Longitude = 10.7522,
            RadiusInMeters = 1000
        });
        
        // Search for addresses by a text query:
        var paginatedResult = await _geoNorge.SearchAddresses(new TextSearchQuery
        {
            SearchTerm = "Karl Johans gate 1, Oslo"
        });

        // If you only want the first result, you can use the QuickSearchLocation extension method:
        var location = await _geoNorge.QuickSearchLocation(new TextSearchQuery
        {
            SearchTerm = "Karl Johans gate 1, Oslo"
        });

        
    }
}
```
