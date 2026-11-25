# 📖 Description

This package contains ASP.NET Core extension methods that provide standardized configuration and setup for web applications. It simplifies the process of configuring common middleware, services, and API conventions.

## 📦 Installation

```bash
dotnet add package Arbeidstilsynet.Common.AspNetCore.Extensions
```

## 🧑‍💻 Usage

### Basic Setup

The package provides extension methods to quickly configure a standard ASP.NET Core web application:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Load your appsettings from configuration
var appSettings = builder.Configuration.GetRequired<MyAppSettings>();

// Configure standard API services
var services = builder.Services;
services.ConfigureApi();
services.ConfigureOpenTelemetry("MyAppName");
services.ConfigureSwagger();
services.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.SetMinimumLevel(LogLevel.Information);
});
services.ConfigureCors(
    appSettings.API.Cors.AllowedOrigins,
    appSettings.API.Cors.AllowCredentials,
    env.IsDevelopment()
);

// Add a memory cached HTTP client
services.AddMemoryCachedHttpClient("MyCachedClient", configure =>
{
    configure.BaseAddress = new Uri("https://api.example.com");
}, cachingOptions =>
{
    cachingOptions.SlidingExpiration = TimeSpan.FromMinutes(5);
    cachingOptions.AbsoluteExpiration = TimeSpan.FromHours(1);
});

// Do the rest of your dependency injection here ...

var app = builder.Build();

// Apply standard API middleware pipeline
app.AddApi(options =>
            options.AddExceptionMapping<MyCustomException>(HttpStatusCode.NotFound)
        );
app.UseCors();
app.AddScalar();

await app.RunAsync();
```

### Authentication and roles

`AddStandardAuth` configures Entra ID (JWT Bearer) authentication from an `AuthConfiguration`. Optionally, map Entra ID groups to roles:

```jsonc
"Authentication": {
  "TenantId": "...",
  "ClientId": "...",
  "Scope": "...",
  "Roles": {
    "Leser": ["<group-guid>", "<group-guid>"],
    "Saksbehandler": ["<group-guid>"]
  }
}
```

Environment variable form: `API__Authentication__Roles__Leser__0=<group-guid>`.

A user gets a role if they are a member of at least one of its groups. Use the roles with `[Authorize(Roles = "Leser")]` or `User.IsInRole("Leser")`. Invalid (non-GUID) group values are ignored, and roles already present on the user (e.g. Entra app roles) are kept.

```csharp
services.AddStandardAuth(appSettings.Authentication);
// ...
app.AddStandardApi(appSettings.Authentication);
app.MapUserRoles(); // GET /roles -> ["Leser", ...]
```

> **NAIS:** the groups must be listed under `spec.azure.application.claims.groups` (with `allowAllUsers: false`), otherwise they are not included in the token.

When `DangerousDisableAuth` is true, all authorization requirements succeed (including role-based and named policies), and `/roles` returns every configured role. Never use this in production.
