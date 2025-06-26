# Introduction

Give a brief introduction to GeoNorge, and explain its purpose.

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

Inject into your class:

```csharp
public class MyService
{
    private readonly IGeoNorge _samplePackage;

    public MyService(IGeoNorge samplePackage)
    {
        _samplePackage = samplePackage;
    }

    public async Task DoSomething()
    {
        var result = await _samplePackage.Get();
        // Use result...
    }
}
```
