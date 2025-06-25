# Introduction

Give a brief introduction to Enhetsregisteret, and explain its purpose.

## 📖 Installation

To install the Enhetsregisteret, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.Enhetsregisteret
```

## 🧑‍💻 Usage

Add to your service collection:

```csharp
public static IServiceCollection AddServices
    (
        this IServiceCollection services,
        DatabaseConfiguration databaseConfiguration
    ) {
        services.AddEnhetsregisteret();
        return services;
    }
```

Inject into your class:

```csharp
public class MyService
{
    private readonly IEnhetsregisteret _samplePackage;

    public MyService(IEnhetsregisteret samplePackage)
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
