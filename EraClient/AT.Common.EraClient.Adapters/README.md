# 📖 Description

Client to interact with our external ERA APIs (OAuth protected)

# 🧑‍💻 Usage

Add service extension in your startup method:

```csharp
public static IServiceCollection AddServices
    (
        this IServiceCollection services,
        DatabaseConfiguration databaseConfiguration
    ) {
        services.AddEraClient();
        return services;
    }
```

Use the need Client via DependencyInjection, available Clients are:

- A
- B
- C

```csharp
public class MyClass(A a) {
    a.Get()
}
```
