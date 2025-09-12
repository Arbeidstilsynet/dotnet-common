# Arbeidstilsynet.Common.Altinn

A collection of common patterns and extensions for cross-cutting concerns for applications which need to interact with Altinns APIs.

## 📖 Installation

To install the package, use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.Altinn
```

## 🚀 Features

- **Extension Methods** for common Altinn operations
- **Abstract Data Processors** for handling data changes
- **Country Code Lookup** with country dialing codes and names
- **Altinn Options Provider** for country dropdowns
- **Embedded Resource Utilities** for JSON data

## 🧑‍💻 Usage

### Dependency Injection Setup

#### Consuming Altinn Instances

```csharp
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var appSettings = builder.Configuration.GetRequired<MyAppSettings>();

// Adds IAltinnAdapter, which abstracts communication with Altinn instances.
services.AddAltinnAdapter(builder.Environment, appSettings.MaskinportenConfiguration);

// Adds Altinn API clients for consuming Altinn services, at a lower level of abstraction than IAltinnAdapter
services.AddAltinnApiClients(builder.Environment, appSettings.MaskinportenConfiguration);

```

## 🤝 Contributing

This library follows standard .NET conventions and includes comprehensive unit tests. When contributing:

1. Add unit tests for new functionality
2. Follow existing code patterns
3. Update documentation for new features
4. Ensure all tests pass

## 📄 License

This project is licensed under the terms specified by Arbeidstilsynet.
