# 📖 Description

Klient som returner dummy data.

# 🧑‍💻 Usage

Bruk via DependencyInjection.

```csharp
public static IServiceCollection AddServices
    (
        this IServiceCollection services,
        DatabaseConfiguration databaseConfiguration
    ) {
        services.AddFooBarKlient();
        return services;
    }
```