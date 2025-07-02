# Introduction

Give a brief introduction to Altinn, and explain its purpose.

## 📖 Installation

To install the Altinn, you can use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.Altinn
```

## 🧑‍💻 Usage

Add to your service collection:

```csharp
public static IServiceCollection AddServices
    (
        this IServiceCollection services,
        DatabaseConfiguration databaseConfiguration
    ) {
        services.AddAltinn();
        return services;
    }
```

Inject into your class:

```csharp
public class MyService
{
    private readonly IAltinn _samplePackage;

    public MyService(IAltinn samplePackage)
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
