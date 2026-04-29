---
name: aspnetcore
description: ASP.NET Core setup, middleware pipeline, exception handling, OpenAPI, authentication, CORS, OpenTelemetry, and health checks using AT.Common.AspNetCore in this repository. Use this skill when bootstrapping a new service, wiring up the middleware pipeline, configuring exception-to-HTTP-status mappings, adding OpenAPI/Scalar, or setting up auth and observability.
license: MIT
metadata:
  domain: backend
  tags: aspnetcore dotnet middleware openapi scalar auth cors opentelemetry health-checks exception-handling
---

# AspNetCore Skill — Arbeidstilsynet/dotnet-common

`Arbeidstilsynet.Common.AspNetCore` (`AT.Common.AspNetCore.Publish`) provides standardised extension methods that wire up an ASP.NET Core service in one place, ensuring consistency across all Arbeidstilsynet backends.

---

## Installation

```bash
dotnet add package Arbeidstilsynet.Common.AspNetCore.Extensions
```

---

## Full Startup Pattern (`Program.cs`)

```csharp
var builder = WebApplication.CreateBuilder(args);

var appSettings = builder.Configuration.GetRequired<MyAppSettings>();

// Standard MVC, problem details, JSON converters
builder.Services.ConfigureStandardApi();

// OpenTelemetry (traces, metrics, logs → OTLP)
builder.Services.ConfigureOpenTelemetry("MyAppName");

// OpenAPI / Scalar
builder.Services.AddStandardOpenApi("MyAppName");

// Entra ID / JWT authentication
builder.Services.AddStandardAuth(appSettings.AuthConfiguration);

// CORS
builder.Services.ConfigureCors(
    appSettings.API.Cors.AllowedOrigins,
    appSettings.API.Cors.AllowCredentials,
    builder.Environment.IsDevelopment()
);

// Optional: startup tasks executed before the health check reports "ready"
builder.Services.AddStartupChecks((provider, cancellationToken) =>
[
    provider.GetRequiredService<IDatabaseMigrator>().MigrateAsync()
]);

var app = builder.Build();

// Middleware pipeline — includes auth, exception handling, routing, controllers,
// health checks, CORS, and Scalar
app.AddStandardApi(
    appSettings.AuthConfiguration,
    configureExceptionHandling: options =>
    {
        options.AddExceptionMapping<MyNotFoundException>(HttpStatusCode.NotFound);
        options.AddExceptionMapping<MyConflictException>(HttpStatusCode.Conflict);
        // Dynamic mapping
        options.AddExceptionMapping<SomeHttpException>(e =>
            e?.StatusCode ?? HttpStatusCode.InternalServerError);
    },
    disableCors: false // optional; set true only during development
);

await app.RunAsync();
```

---

## Key Service-Collection Methods

| Method | What it registers |
|--------|-------------------|
| `ConfigureStandardApi()` | Calls `ConfigureStandardMvc()` + `AddStandardHealthChecks()` — convenience one-liner |
| `ConfigureStandardMvc()` | Controllers + JSON options (`JsonStringEnumConverter`, `JsonStringUriConverter`, camelCase), `RequestValidationFilter`, `ProblemDetails`. Returns `IMvcBuilder` for further configuration |
| `AddStandardOpenApi(appName)` | OpenAPI document with title/version + enum-as-string schema transformer |
| `AddStandardAuth(authConfig)` | JWT Bearer (Entra ID) when `DisableAuth = false`; permissive allow-all policy otherwise |
| `ConfigureOpenTelemetry(appName)` | ASP.NET Core + HttpClient instrumentation, OTLP export for traces/metrics/logs |
| `ConfigureCors(origins, credentials, isDev)` | Default CORS policy (any origin in dev if no origins provided) |
| `ConfigureCors(corsConfiguration, isDev)` | Same as above but accepts a `CorsConfiguration` record with `AllowedOrigins` and `AllowCredentials` properties |
| `AddStartupChecks(delegate)` | Runs async tasks before `StartupHealthCheck` reports healthy |
| `AddStandardHealthChecks()` | `/healthz/ready` + `/healthz/live` health check endpoints |

---

## Key App (Middleware) Methods

| Method | What it adds |
|--------|--------------|
| `AddStandardApi(authConfig, configureExceptions, disableCors)` | Full pipeline: authentication → exception handler → HTTPS redirection → routing → authorization → controllers → health checks → CORS → Scalar |
| `AddScalar()` | `GET /openapi/v1.json` + Scalar UI at `/scalar/v1` |

---

## Exception Handling

Exceptions are mapped to HTTP status codes through `ExceptionHandlingOptions`. Unhandled exceptions are serialised as RFC 7807 `ProblemDetails`.

Built-in default mappings (always active):

| Exception | Status code |
|-----------|-------------|
| `ArgumentException` | 400 Bad Request |
| `FormatException` | 400 Bad Request |
| `BadHttpRequestException` | 400 Bad Request |
| *(anything else)* | 500 Internal Server Error |

Add custom mappings in the `AddStandardApi` call:

```csharp
// Static mapping
options.AddExceptionMapping<VarselNotFoundException>(HttpStatusCode.NotFound);

// Dynamic mapping (status code derived from exception instance)
options.AddExceptionMapping<AltinnHttpRequestException>(e =>
    e?.StatusCode ?? HttpStatusCode.InternalServerError);
```

---

## Configuration Validation

`GetRequired<T>` binds a configuration section **and validates it** using `DataAnnotations` (recursively). Throws `InvalidOperationException` with clear messages if any required fields are missing or invalid.

```csharp
var appSettings = builder.Configuration.GetRequired<MyAppSettings>();
```

```csharp
public record MyAppSettings
{
    [Required]
    public required string ServiceName { get; init; }

    [Required]
    public required DatabaseSettings Database { get; init; }
}
```

---

## Authentication (`AuthConfiguration`)

```csharp
public record AuthConfiguration
{
    [ConfigurationKeyName("DangerousDisableAuth")]
    public bool DisableAuth { get; init; }   // JSON key is "DangerousDisableAuth"; set true in dev/test only
    public required string TenantId { get; init; }
    public required string ClientId { get; init; }
    public required string Scope { get; init; }
}
```

> **Note:** The `DisableAuth` property is bound from the JSON key `DangerousDisableAuth` (via `[ConfigurationKeyName]`). Ensure your `appsettings.json` uses `"DangerousDisableAuth": true` (not `"DisableAuth"`) when you need to disable auth for local development.

- When `DisableAuth = false` (production): JWT Bearer + Entra ID token validation.
- When `DisableAuth = true`: a permissive "allow all" policy is registered — a warning is logged. **Never use in production.**
- OpenAPI security schemes (`Bearer` + `OAuth2 client_credentials`) are only added when `DisableAuth = false`.

---

## OpenTelemetry Sources

The following `ActivitySource` names are pre-registered for tracing:

| Source | Convention |
|--------|-----------|
| `Domain.Logic` | hexarch ≥ v3 |
| `API.Adapters` | hexarch ≥ v3 |
| `Infrastructure.Adapters` | hexarch ≥ v3 |
| `App` | hexarch < v3 |
| `Infrastructure` | hexarch < v3 |

Health check requests (`/healthz`) are excluded from traces.

---

## Health Checks

| Endpoint | Behaviour |
|----------|-----------|
| `GET /healthz/ready` | Unhealthy until all `StartupChecks` complete; returns JSON report |
| `GET /healthz/live` | Always healthy (liveness probe) |

---

## JSON Conventions

Both `System.Text.Json` HTTP JSON options and MVC JSON options are configured identically:

- `JsonStringEnumConverter` — enums serialised as strings
- `JsonStringUriConverter` — `Uri` values serialised as strings
- `PropertyNamingPolicy = CamelCase`

---

## Adding a New Service — Checklist

1. `dotnet add package Arbeidstilsynet.Common.AspNetCore.Extensions`
2. Call `ConfigureStandardApi()` + `AddStandardOpenApi(appName)` + `ConfigureOpenTelemetry(appName)` on `builder.Services`
3. Call `AddStandardAuth(authConfig)` if the service requires authentication
4. Call `ConfigureCors(...)` if the service is accessed from a browser
5. Call `AddStartupChecks(...)` for any DB migrations or cache warm-up
6. Call `app.AddStandardApi(authConfig, exceptionMappings)` to wire the middleware pipeline
7. Define typed exceptions in your domain layer and map them to HTTP status codes in the `AddStandardApi` call
