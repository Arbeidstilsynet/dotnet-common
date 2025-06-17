# Introduction

Give a brief introduction to BlubExtensions, and explain its purpose.

## 📖 Installation

To install the BlubExtensions, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.BlubExtensions
```

## 🧑‍💻 Usage

Add to your service collection:

```csharp
public static IServiceCollection AddServices
    (
        this IServiceCollection services,
        DatabaseConfiguration databaseConfiguration
    ) {
        services.AddBlubExtensions();
        return services;
    }
```

Inject into your class:

```csharp
public class MyService
{
    private readonly IBlubExtensions _samplePackage;

    public MyService(IBlubExtensions samplePackage)
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
