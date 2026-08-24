# Arbeidstilsynet.Common.Altinn

A collection of common patterns and extensions for cross-cutting concerns for applications which need to interact with Altinns APIs.

## 📖 Installation

To install the package, use the following command in your terminal:

```bash
dotnet add package Arbeidstilsynet.Common.Altinn
```

## 🚀 Features

- **Extension Methods** for common Altinn operations
- **Altinn Adapter**  Provides a high-level abstraction for Arbeidstilsynet’s integration needs, streamlining communication with Altinn instances.
- **Altinn API Clients** Robust REST API clients for direct and flexible interaction with Altinn’s services, supporting both general and advanced use cases.
- **Generated low-level clients** built with [Kiota](https://learn.microsoft.com/openapi/kiota/) from Altinn's OpenAPI specifications, exposed for local adaptation.

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

### Choosing which Altinn to talk to

Which Altinn instance you reach is stated explicitly rather than inferred, because
several host environments can legitimately target either TT02 or a local Altinn.

| Host environment | Altinn target | Base URL overrides |
| --- | --- | --- |
| `Production` | Always production | Rejected — registration throws |
| `Staging` | TT02 by default | Allowed, logged as a warning at startup |
| Anything else | **Must be specified** | Allowed |

So in Development, Test, QA and similar you must say what you mean:

```csharp
services.AddAltinnAdapter(
    builder.Environment,
    appSettings.MaskinportenConfiguration,
    new AltinnConfiguration { Environment = AltinnEnvironment.Tt02 }
);
```

Use `AltinnEnvironment.Local` to target a locally running Altinn on
`local.altinn.cloud:8000`. This also selects the local test-token provider, so the
tokens in use always match the instance being called.

To point the clients at a mock server, override the platform URL. The API-specific
base paths are appended automatically, so a single override covers every platform API:

```csharp
new AltinnConfiguration
{
    Environment = AltinnEnvironment.Tt02,
    Overrides = new AltinnUrlOverrides { PlatformUrl = new Uri("http://localhost:1234/") },
}
```

### Working with the generated clients

The ports return Kiota-generated models, and the generated clients themselves are
registered so you can reach endpoints the high-level clients do not wrap:

```csharp
public class MyService(IAltinnStorageClient storageClient, StorageApiClient generatedClient);
```

Two things to know about the generated models:

- They are Kiota `IParsable` types, not `System.Text.Json` POCOs. Deserialising them
  with `JsonSerializer` silently produces empty objects.
- Errors surface as Kiota's `ApiException`, which carries `ResponseStatusCode`.

## 🔄 Regenerating the clients

The OpenAPI specifications live alongside the source and ship with the package.
After refreshing one from Altinn, regenerate with:

```bash
npm run generate:client
```

This normalises the specifications first, which matters: Kiota only wires a client's
base URL up when the specification declares a server, and the Altinn apps and
authentication specifications declare none.

Correspondence and Dialogporten publish a specification per environment. The clients
are generated from the TT02 specifications, which are a strict superset of the
production ones — generating a client per environment would split the public model
types. The consequence is that a production application can call an endpoint that has
not yet been released to production and receive a `404`. To check that the assumption
still holds:

```bash
npm run check:spec-drift
```

## 🤝 Contributing

This library follows standard .NET conventions and includes comprehensive unit tests. When contributing:

1. Add unit tests for new functionality
2. Follow existing code patterns
3. Update documentation for new features
4. Ensure all tests pass

## 📄 License

This project is licensed under the terms specified by Arbeidstilsynet.
