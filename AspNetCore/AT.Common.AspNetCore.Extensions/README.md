# AT.Common.AspNetCore.Extensions

## 📖 Description

Extension methods for validation and configuration of a standard AspNetCore API.

## 🧑‍💻 Usage

Import package

```xml
    <PackageReference Include="Arbeidstilsynet.Common.AspNetCore.Extensions"
    />
```

```csharp
using Arbeidstilsynet.Common.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Resolve and validate your appsettings from configuration
var appSettings = builder.Configuration.GetRequired<AppSettings>();

// Configure AspNetCore API
builder.Services.ConfigureApi();
builder.Services.ConfigureOpenTelemetry("MyFancyApp");
builder.Services.ConfigureSwagger();

var app = builder.Build();

// Add middleware (Controllers, Exception handling, Scalar)
app.AddApi(options =>
            options.AddExceptionMapping<SakNotFoundException>(HttpStatusCode.NotFound)
        );
        app.AddScalar();

app.Run();
```
