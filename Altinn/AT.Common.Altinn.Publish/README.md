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
- **Per-client configuration** Register only the APIs you use, each with its own Maskinporten scopes.

## 🧑‍💻 Usage

### Dependency Injection Setup

Register the APIs your application actually uses:

```csharp
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var appSettings = builder.Configuration.GetRequired<MyAppSettings>();

services
    .AddAltinn(builder.Environment, appSettings.MaskinportenConfiguration)
    .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"])
    .AddEvents(o => o.Scopes = ["altinn:events.subscribe"])
    .AddSubscriptionAdapter();
```

`AddAltinn` registers the shared plumbing — configuration, URL resolution and the token
pipeline — but nothing that talks to an Altinn API. Each `Add*` call opts into one API:

| Call | Registers |
| --- | --- |
| `AddStorage()` | `IAltinnStorageClient` |
| `AddEvents()` | `IAltinnEventsClient` |
| `AddApps()` | `IAltinnAppsClient` |
| `AddCorrespondence()` | `IAltinnCorrespondenceClient` |
| `AddDialogporten()` | `IAltinnDialogportenClient` |
| `AddSubscriptionAdapter()` | `IAltinnSubscriptionAdapter`, plus storage and events |
| `AddStorageAdapter()` | `IAltinnStorageAdapter`, plus storage |
| `AddMeldingerAdapter()` | `IAltinnMeldingerAdapter`, plus correspondence |
| `AddAllClients()` | every API client |

Adding a client twice is harmless, and configuration is independent of call order — so
an adapter can pull in a client and you can still configure it afterwards:

```csharp
services
    .AddAltinn(builder.Environment, appSettings.MaskinportenConfiguration)
    .AddSubscriptionAdapter()
    .AddStorage(o => o.Scopes = ["altinn:serviceowner/instances.read"]);
```

To register everything in one call, as in earlier versions:

```csharp
services.AddAltinnAdapter(builder.Environment, appSettings.MaskinportenConfiguration);
services.AddAltinnApiClients(builder.Environment, appSettings.MaskinportenConfiguration);
```

### Authentication

Altinn grants scopes per API, so each client requests a token carrying only the scopes it
was registered with. Tokens are cached per distinct scope set, so clients sharing a scope
set share a token.

Scopes come from the client's own `Scopes`, falling back to
`MaskinportenConfiguration.Scopes`:

```csharp
services
    .AddAltinn(builder.Environment, new MaskinportenConfiguration
    {
        // Used by any client that does not state its own scopes.
        Scopes = ["altinn:serviceowner/instances.read"],
        // ...credentials
    })
    .AddStorage()                                        // uses the shared scopes
    .AddEvents(o => o.Scopes = ["altinn:events.subscribe"]);  // uses its own
```

Both are optional individually, but a registered client must end up with at least one
scope. If it does not, registration fails at startup naming the client involved, rather
than surfacing as a rejected token on the first request. Targeting
`AltinnEnvironment.Local` is exempt, since a local Altinn issues test tokens directly and
never involves Maskinporten.

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
services.AddAltinn(
    builder.Environment,
    appSettings.MaskinportenConfiguration,
    new AltinnConfiguration { Environment = AltinnEnvironment.Tt02 }
);
```

Use `AltinnEnvironment.Local` to target a locally running Altinn on
`local.altinn.cloud:8000`. This also selects the local test-token provider, so the
tokens in use always match the instance being called.

To point every platform client at a mock server, override the platform URL. The
API-specific base paths are appended automatically:

```csharp
new AltinnConfiguration
{
    Environment = AltinnEnvironment.Tt02,
    Overrides = new AltinnUrlOverrides { PlatformUrl = new Uri("http://localhost:1234/") },
}
```

To move a single client, set its `BaseUrl` instead:

```csharp
.AddStorage(o => o.BaseUrl = new Uri("http://localhost:1234/storage/api/v1"))
```

Both forms obey the same rule: rejected in Production, warned about elsewhere.

### Models and errors

The clients return the package's own models from `Model.Api.Response` and
`Model.Adapter`. They are plain records, so they serialise with `System.Text.Json` the
way you would expect and are safe to return from your own API.

The Kiota-generated types are an implementation detail and are `internal`. They are Kiota
`IParsable` types rather than `System.Text.Json` POCOs, so exposing them would mean
consumers could not serialise them faithfully — enums would become integers and free-form
maps such as `dataValues` would gain an extra `additionalData` level that no serializer
option removes.

Errors surface as Kiota's `ApiException`, which carries `ResponseStatusCode`. Where a
missing resource is an expected outcome the adapters translate it rather than throwing:
`GetAltinnSubscription` and `GetCorrespondence` return `null`, and
`UnsubscribeForCompletedProcessEvents` returns `false`. Any other failure propagates.

When Altinn returns a problem details document, `GetAltinnProblemDetails()` reads it off
the exception:

```csharp
try
{
    await storageClient.GetInstance(instanceGuid);
}
catch (ApiException e)
{
    var problem = e.GetAltinnProblemDetails();

    logger.LogError(
        "Altinn returned {Status}: {Title} — {Detail} (trace {TraceId})",
        problem?.Status, problem?.Title, problem?.Detail, problem?.TraceId);
}
```

It returns `null` when the response carried no problem details — either because the
endpoint does not declare one for that status code, or because the request failed before a
response arrived. Beyond the RFC 9457 fields, `AltinnProblemDetails` exposes the
Altinn-specific `Code`, `ErrorCode`, `StatusDescription`, `TraceId`, `ValidationErrors` and
`Errors`, each populated by the APIs that return them.

Generation is scoped to the functional area each client serves — instances for storage and
apps, subscriptions for events, and dialogs for Dialogporten. To reach an endpoint outside
those areas, widen the relevant `--include-path` filter in `package.json`, regenerate, and
expose it through the corresponding port.

## 🔄 Regenerating the clients

The OpenAPI specifications live alongside the source. They are inputs to generation only and are
not shipped inside the package.
After refreshing one from Altinn, regenerate with:

```bash
npm run generate:client
```

This normalises the specifications first, which matters: Kiota only wires a client's
base URL up when the specification declares a server, and the Altinn apps and
authentication specifications declare none.

Correspondence and Dialogporten publish a specification per environment. The clients
are generated from the TT02 specifications, which are a strict superset of the
production ones — generating a client per environment would split the generated model
types in two. The consequence is that a production application can call an endpoint that
has not yet been released to production and receive a `404`. To check that the assumption
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
