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

### Changed

- **BREAKING**: Replaced the hand-written Altinn HTTP clients and hand-copied models with [Kiota](https://learn.microsoft.com/openapi/kiota/)-generated clients and models, generated from Altinn's OpenAPI specifications. The specifications ship with the package, and the generated clients are exposed for local adaptation under `Arbeidstilsynet.Common.Altinn.Storage`, `.Events`, `.Authentication`, `.Correspondence`, `.Dialogporten` and `.Apps`.
- **BREAKING**: The `IAltinn*Client` and adapter ports are unchanged in shape but now exchange the generated models. `AltinnInstance` becomes `Storage.Models.Instance`, `AltinnQueryResponse<T>` becomes `Storage.Models.InstanceQueryResponse`, `AltinnSubscription` becomes `Events.Models.Subscription`, `AltinnCorrespondenceOverview` becomes `Correspondence.Models.CorrespondenceOverviewExt`, `CorrespondenceResponse` becomes `Correspondence.Models.InitializeCorrespondencesResponseExt`, and `DialogportenLookupResponse` becomes `Dialogporten.Models.V1CommonIdentifierLookup_ServiceOwnerIdentifierLookup`.
- **BREAKING**: `AltinnConfiguration` no longer exposes `AuthenticationUrl`, `StorageUrl`, `EventUrl`, `CorrespondenceUrl`, `DialogportenUrl` or `AppBaseUrl`. It now carries an explicit `Environment` (`AltinnEnvironment.Production`, `Tt02` or `Local`) from which every base URL is derived, plus an optional `Overrides` for testing against a mock server.
- **BREAKING**: The Altinn instance to target is no longer inferred from the host environment alone. Production always targets production and rejects overrides; Staging defaults to TT02 and logs any override as a startup warning; every other host environment (Development, Test, QA, …) must state its target explicitly and throws at registration if it does not. `AltinnEnvironment.Local` preserves the previous local-development behaviour.
- **BREAKING**: `MaskinportenConfiguration.MaskinportenUrl` moved to `AltinnUrlOverrides.MaskinportenUrl`, so that every URL override is subject to the same rules.
- **BREAKING**: The token provider now follows the resolved Altinn target rather than `IsDevelopment()`. A local test token can no longer be paired with TT02 URLs.
- **BREAKING**: `IAltinnEventsClient.Unsubscribe` returns `Task` rather than `Task<HttpResponseMessage>`. `IAltinnAdapter.UnsubscribeForCompletedProcessEvents` accordingly returns `false` only when the subscription does not exist, and propagates any other failure instead of reporting `false` for every non-success status.
- **BREAKING**: Errors surface as Kiota's `ApiException` rather than `HttpRequestException` or `AltinnHttpRequestException`. Both adapters still return `null` for a missing resource.
- **BREAKING**: All client and adapter methods accept an optional `CancellationToken`.
- `CorrespondenceRequest` now carries the generated correspondence models, and its multipart upload uses the field names and casing declared by the specification.

### Removed

- **BREAKING**: `HostEnvironmentExtensions` (`CreateDefaultAltinnConfiguration`, `GetMaskinportenUrl`, `GetAltinnPlattformUrl`, `GetAltinnAppBaseUrl`). Base URLs are resolved from `AltinnConfiguration.Environment` instead.
- **BREAKING**: The hand-written request and response models under `Model/Api`, superseded by the generated ones.

### Fixed

- The generated apps and authentication clients could not build a URL at all, because Kiota only emits the `baseurl` path parameter when a specification declares a server and neither specification does. The token exchange sits on the critical path for every authenticated call, so this would have failed on the first request. The specifications are normalised as part of client generation.
- The generated storage and apps clients indexed instances by a `Guid` where Altinn expects an integer party id, because both specifications contain paths that collide at the same position in the request-builder tree and Kiota silently merged them.
- The Maskinporten token request ignored its `CancellationToken`, because the bespoke HTTP layer it used had no way to accept one. That layer is now gone entirely.
- The correspondence multipart upload sent its form fields in camelCase where the specification declares PascalCase. Form binding is case-insensitive, so this was latent rather than broken.

### Notes

- Correspondence and Dialogporten publish a specification per environment. Both TT02 specifications are a strict superset of their production counterparts, so a single client is generated per API from TT02; generating one per environment would split the public model types. A production application may therefore see a `404` from an endpoint that has not yet been released to production. `npm run check:spec-drift` fails if production ever declares a path or schema that TT02 lacks.
- The generated models are Kiota `IParsable` types, not `System.Text.Json` POCOs. Deserialising them with `JsonSerializer` silently yields empty objects.

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
