# 📖 Description

This package contains ASP.NET Core extension methods that provide standardized configuration and setup for web applications. It simplifies the process of configuring common middleware, services, and API conventions.

## 🚀 Features

- **`ConfigureStandardApi(string appName)`**: Configures standard API services including logging, CORS, authentication, and other common services
- **`AddStandardApi()`**: Sets up the standard middleware pipeline for API applications
- **`GetRequired<T>()`**: Extension for configuration binding with validation

## 📦 Installation

```bash
dotnet add package AT.Common.AspNetCore
```

## 🧑‍💻 Usage

### Basic Setup

The package provides extension methods to quickly configure a standard ASP.NET Core web application:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Load your appsettings from configuration
var appSettings = builder.Configuration.GetRequired<MyAppSettings>();
var services = builder.Services;
var env = builder.Environment;

// Configure standard API services
services.ConfigureStandardApi(IAssemblyInfo.AppName);

// Add your domain and infrastructure services (optional)
services.AddDomain();
services.AddInfrastructureServices(appSettings.DatabaseConfiguration);

var app = builder.Build();

// Configure development-specific middleware
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Apply standard API middleware pipeline
app.AddStandardApi();

await app.RunAsync();
```