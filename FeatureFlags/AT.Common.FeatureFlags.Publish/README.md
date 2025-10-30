# Introduction

Give a brief introduction to FeatureFlags, and explain its purpose.

## 📖 Installation

To install the FeatureFlags, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.FeatureFlags
```

## 🧑‍💻 Usage

Add to your service collection:

```csharp
public static IServiceCollection AddServices
    (
        this IServiceCollection services,
        DatabaseConfiguration databaseConfiguration
    ) {
        services.AddFeatureFlags();
        return services;
    }
```

Inject into your class:

```csharp
public class MyService
{
    private readonly IFeatureFlags _samplePackage;

    public MyService(IFeatureFlags samplePackage)
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
