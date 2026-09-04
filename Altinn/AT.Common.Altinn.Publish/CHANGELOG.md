# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added <!-- for new features. -->

### Changed <!--  for changes in existing functionality. -->

### Deprecated <!--  for soon-to-be removed features. -->

### Removed <!-- for now removed features. -->

### Fixed <!-- for any bug fixes. -->

### Security <!-- in case of vulnerabilities. -->

## 4.0.0

### Added

- Granular registration. `services.AddAltinn(...)` returns a builder on which each API is opted into individually — `AddStorage()`, `AddEvents()`, `AddApps()`, `AddCorrespondence()`, `AddDialogporten()`, `AddSubscriptionAdapter()`, `AddStorageAdapter()`, `AddMeldingerAdapter()` — so an application only registers, and only authenticates against, the APIs it uses. `AddAllClients()` registers them all. Adding a client twice is a no-op, and configuration is independent of call order, so a client pulled in by an adapter can still be configured afterwards.
- Per-client configuration via `AltinnClientOptions`, carrying `Scopes` and `BaseUrl` for a single API. A per-client `BaseUrl` is subject to the same rule as `AltinnUrlOverrides`: rejected in Production, logged as a warning elsewhere.
- Per-client Maskinporten scopes. Each client requests a token carrying only the scopes it was registered with, rather than the union of everything the application is entitled to, and Maskinporten tokens are cached per distinct scope set. A registered client that ends up with neither its own scopes nor the shared fallback fails validation at startup, naming the client involved. Targeting `AltinnEnvironment.Local` is exempt, since it issues test tokens without involving Maskinporten.
- `ApiException.GetAltinnProblemDetails()`, returning the problem details document Altinn sent with an error response as an `AltinnProblemDetails`. The generated clients already parse these and throw them as the exception itself, but the parsed types are internal; this exposes their contents, including the Altinn-specific `Code`, `ErrorCode`, `StatusDescription`, `TraceId`, `ValidationErrors` and `Errors`. Returns `null` when the response carried no problem details.

### Changed

- **BREAKING**: Replaced the hand-written Altinn HTTP clients with [Kiota](https://learn.microsoft.com/openapi/kiota/)-generated clients, generated from Altinn's OpenAPI specifications. The generated types are `internal`: they are Kiota `IParsable` types rather than `System.Text.Json` POCOs, so exposing them would leave consumers unable to serialise them faithfully. The `IAltinn*Client` and adapter ports keep their shape and continue to exchange this package's own models.
- **BREAKING**: The public models are now records with `init` accessors, and response models mirror the nullability of the underlying Altinn schema — most properties are nullable. They are reduced to the subset of fields this package exposes, so `AltinnInstance` no longer carries `SelfLinks`, `DueBefore`, `VisibleAfter`, `Status`, `PresentationTexts`, `Refs` or `DeleteStatus`, and `DataElement` no longer carries `BlobStoragePath` or `Locked`.
- **BREAKING**: `AltinnConfiguration` no longer exposes `AuthenticationUrl`, `StorageUrl`, `EventUrl`, `CorrespondenceUrl`, `DialogportenUrl` or `AppBaseUrl`. It now carries an explicit `Environment` (`AltinnEnvironment.Production`, `Tt02` or `Local`) from which every base URL is derived, plus an optional `Overrides` for testing against a mock server.
- **BREAKING**: The Altinn instance to target is no longer inferred from the host environment alone. Production always targets production and rejects overrides; Staging defaults to TT02 and logs any override as a startup warning; every other host environment (Development, Test, QA, …) must state its target explicitly and throws at registration if it does not. `AltinnEnvironment.Local` preserves the previous local-development behaviour.
- **BREAKING**: `MaskinportenConfiguration.MaskinportenUrl` moved to `AltinnUrlOverrides.MaskinportenUrl`, so that every URL override is subject to the same rules.
- **BREAKING**: `MaskinportenConfiguration.Scopes` is optional, acting as the default for clients that do not state their own scopes.
- **BREAKING**: `IMaskinportenClient.GetToken` and `IAltinnTokenProvider.GetToken` take the scopes to request.
- **BREAKING**: The token provider now follows the resolved Altinn target rather than `IsDevelopment()`. A local test token can no longer be paired with TT02 URLs.
- **BREAKING**: `IAltinnAdapter` and its implementation are renamed to `IAltinnSubscriptionAdapter` and `AltinnSubscriptionAdapter`; the fluent registration method is now `AddSubscriptionAdapter()`.
- **BREAKING**: `IAltinnEventsClient.Unsubscribe` returns `Task` rather than `Task<HttpResponseMessage>`. `IAltinnSubscriptionAdapter.UnsubscribeForCompletedProcessEvents` accordingly returns `false` only when the subscription does not exist, and propagates any other failure instead of reporting `false` for every non-success status.
- **BREAKING**: `IAltinnStorageClient.GetInstance` takes a `Guid` rather than an `InstanceRequest`, and calls the storage API's guid-only endpoint. Altinn keeps the two-segment form that also requires an instance owner party id only for backwards compatibility. `GetInstanceData` still takes the party id, because data elements are only addressable through the older form.
- **BREAKING**: Errors surface as Kiota's `ApiException` rather than `HttpRequestException` or `AltinnHttpRequestException`. Both adapters still return `null` for a missing resource.
- **BREAKING**: All client and adapter methods accept an optional `CancellationToken`.
- `InstanceQueryParameters` properties are `init`-only and its optional filters are nullable, matching the fact that they are optional.
- The correspondence multipart upload uses the field names and casing declared by the specification.

### Removed

- **BREAKING**: `HostEnvironmentExtensions` (`CreateDefaultAltinnConfiguration`, `GetMaskinportenUrl`, `GetAltinnPlattformUrl`, `GetAltinnAppBaseUrl`). Base URLs are resolved from `AltinnConfiguration.Environment` instead.
- **BREAKING**: `MappedQueryParameterAttribute` and `MappedRequestHeaderParameterAttribute`. Query parameters are now mapped explicitly rather than by reflection over attributes.
- **BREAKING**: `AltinnHttpRequestException`, which belonged to the retired hand-written HTTP layer and can no longer be thrown. Catch Kiota's `ApiException` and call `GetAltinnProblemDetails()` instead.

### Fixed

- The generated apps and authentication clients could not build a URL at all, because Kiota only emits the `baseurl` path parameter when a specification declares a server and neither specification does. The token exchange sits on the critical path for every authenticated call, so this would have failed on the first request. The specifications are normalised as part of client generation.
- The generated storage and apps clients indexed instances by a `Guid` where Altinn expects an integer party id, because both specifications contain paths that collide at the same position in the request-builder tree and Kiota silently merged them.
- The Maskinporten token request ignored its `CancellationToken`, because the bespoke HTTP layer it used had no way to accept one. That layer is now gone entirely.
- The correspondence multipart upload sent its form fields in camelCase where the specification declares PascalCase. Form binding is case-insensitive, so this was latent rather than broken.
- `InitializeCorrespondenceContent.Language` defaults to `nb`, as the correspondence specification declares. Leaving it unset previously omitted the field from the request.

### Notes

- The generated clients are scoped to the functional area each high-level client serves rather than the whole specification: instances for storage and apps, subscriptions for events, and dialogs for Dialogporten. Endpoints outside those areas are not generated, so reaching one means widening the filter in `package.json`, regenerating, and exposing it through the corresponding port.
- The OpenAPI specifications are no longer shipped inside the package. They are inputs to client generation only, and remain versioned in the repository alongside the code generated from them.
- `AltinnCloudEvent` remains hand-written and must not be replaced by the generated `Events.Models.CloudEvent`: the events specification models `specversion` as an object rather than the string Altinn actually sends, so binding a real event to the generated model throws and an `[ApiController]` returns a 400. That would break subscription setup outright, since Altinn requires a success response to its `platform.events.validatesubscription` event before activating a subscription.
- `InstanceQueryParameters` also remains hand-written, because Kiota omits header parameters from the generated query-parameter class and the storage query needs `X-Ai-InstanceOwnerIdentifier`.
- Correspondence and Dialogporten publish a specification per environment. Both TT02 specifications are a strict superset of their production counterparts, so a single client is generated per API from TT02; generating one per environment would split the model types. A production application may therefore see a `404` from an endpoint that has not yet been released to production. `npm run check:spec-drift` fails if production ever declares a path or schema that TT02 lacks.

## 3.2.4

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 3.2.3

### Fixed

- fix(altinn): `GetNonCompletedInstances` now handles per-instance summary failures by logging a warning and continuing with remaining instances.
- fix(altinn): replaced generic `InvalidOperationException` throws in instance summary flow with custom exceptions containing instance-specific context.

## 3.2.2

### Fixed

- fix(altinn): `GetSpecification` no longer throws `NullReferenceException` when `DataValues` is `null` on an `AltinnInstance`

## 3.2.1

### Changed

- chore(altinn): Updated default URL mapping for development environment to match new altinn studiocli setup.

## 3.2.0

### Added

- feat(altinn): Added `Validated` property to `AltinnSubscription` response model, indicating whether the subscription has been validated by Altinn.

## 3.1.1

### Fixed

- fix(altinn): added necessary null checks when converting an altinn instance to metadata

## 3.1.0

### Added

- feat(altinn): Enhace `AltinnMetaData` to include dialogId and all DataValues which come from an altinn instance.
- feat(altinn): Added ``AltinnDialogportenClient`` to support communication with dialogporten. Can be injected via ``IAltinnDialogportenClient`` and requires a maskinporten integration with the following altinn scope to work: `altinn:serviceowner`. Right now the client only supports one method to retrieve a dialogId based on an instanceId.
- fix(extensions): make all methods in `HostEnvironmentExtensions` publicly available

## 3.0.0

### Removed

- **BREAKING**: Removed dependency on `Arbeidstilsynet.Common.AspNetCore.Extensions`

### Changed

- Added `FrameworkReference` to `Microsoft.AspNetCore.App` for ASP.NET Core types

## 2.6.1

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 2.6.0

### Added

- feat(altinn): Added ``AltinnCorrespondenceClient`` to support posting correspondences, with and without attachments. Can be injected via ``IAltinnCorrespondenceClient`` and requires a maskinporten integration with the following altinn scope to work: `altinn:correspondence.write`. It is also possible to retrieve a correspondence by ID.
- feat(altinn): Added ``AltinnMeldingerAdapter`` to make usage of the ``IAltinnCorrespondenceClient`` easier. Can be injected via ``IAltinnMeldingerAdapter``.

### Changed

- feat(maskinporten): Updated MaskinportenConfiguration to also support jwt token generation which includes a key identifier (`kid`) instead of only supporting identification by a certificate chain.

## 2.5.0

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 2.4.2

### Fixed

- Fixed a bug where a partial altinn configuration would overwrite defaults.

## 2.4.1

### Changed

- chore: moved package to nuget.org

## 2.4.0

### Added

- feat(altinn): Differentiate between structured data and the main content of the instance.
  - Implements the StructuredData configuration from AltinnApp 2.4.0.
  - Adds DataType and Id from the Altinn DataElement to the FileMetadata of the AltinnDocument.

## 2.3.0

### Added

- feat(altinn): Enhanced ``AltinnEventsClient`` to support retrieving details for a subscriptions. Also updated ``IAltinnAdapter`` to implement a convenience method for getting these details.

## 2.2.0

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 2.1.0

### Added

- feat(altinn): Enhanced ``AltinnEventsClient`` to support Unsubscribing. Also updated ``IAltinnAdapter`` to implement a convenience method for unsubscribing.

## 2.0.2

### Added

- feat(altinn): Added function to return only metadata for non completed instances (in order to not download attachments)

## 2.0.1

### Changed

- changed(deps): Updated internal package referances (remove prerelease version)

## 2.0.0

### Changed

- changed(deps): Major dotnet updated (v10)

## 1.4.0

### Changed

- changed: using "structured-data" as default dataType for MainDocument.

## 1.3.2

### Added

- chore: added extension methods to create a Dictionary based on `AltinnMetadata`.

## 1.3.1

### Fixed

- fix: add nullable declarations to `AltinnCloudEvent` dto to enable usage in a dotnet Controller.

## 1.3.0

### Changed

- changed: split up package to only contain logic to communicate with Altinns API. All logic with is only relevant for altinn apps was moved to the new AltinnApp package.

## 1.2.0

### Changed

- fix(deps): use correct version range to only support Microsoft version 8.* packages

## 1.1.0

### Fixed

- fix: move complete action to new "Apps Client" since all mutable instance actions must go through Apps Api instead of Storage Api

## 1.0.0

### Added

- feat: add altinn token provider and clients to handle token exchange

## 0.0.4

### Changed

- chore: enhance altinn metadata with process started / ended

## 0.0.3

### Changed

- chore(renovate): all non major update

## 0.0.2

### Changed

- chore(altinn-adapter): update FileMetadata model to return an enum of FileScanResults instead of a string

## 0.0.1

### Added

- **API Clients** to be used to consume public Altinn APIs
  - `Storage Api Client` for the storage API (Instance Data)
  - `Event Api Client` for the event API (Subscriptions)
- **Extension Methods** for common Altinn operations
  - `InstanceExtensions` - Extract GUID, app name, and party ID from instances
  - `DataClientExtensions` - Simplified form data retrieval and element deletion
  - `AssemblyExtensions` - Load and deserialize embedded JSON resources
- **Abstract Data Processors** for handling form data changes
  - `BaseDataProcessor<T>` - Base class for type-specific data processing
  - `MemberProcessor<T, TMember>` - Process changes to specific object members
  - `ListProcessor<T, TItem>` - Handle list/collection changes with item-level processing
- **Country Code Lookup Service** (`ILandskodeLookup`)
  - Country anmes and dial codes for 238 countries
- **Altinn Options Provider** for country dropdowns
  - `LandOptions` for Altinn dropdowns etc.
