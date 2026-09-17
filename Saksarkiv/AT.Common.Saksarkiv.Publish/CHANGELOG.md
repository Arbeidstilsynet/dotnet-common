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

## 2.0.0

### Added

- Added a generated client for the new Saksarkiv **API v3** (`SaksarkivClientV3`, namespace
  `Arbeidstilsynet.Common.Saksarkiv.V3`), generated with Kiota from `openApiV3.json`.
- Added `AddSaksarkivClientV2(...)` to register the legacy v2 client (`SaksarkivClient`) on demand.
- Added a typed `SaksarkivClientV3.OpprettSakAsync(...)` extension (with a `SaksarkivFile` upload
  type) that wraps the multipart plumbing for creating a case, since the v3 spec models the create
  payload as an (untyped) query parameter.

### Changed

- **Breaking:** `AddSaksarkivClient(...)` now registers the **v3** client (`SaksarkivClientV3`) as
  the default instead of the v2 client. Consumers that still need v2 must call
  `AddSaksarkivClientV2(...)` and resolve `SaksarkivClient`.
- The `Saksarkiv` health check now probes the v3 endpoint `GET /api/v3/metadata/tilgangskoder`
  (v3 has no dedicated health endpoint) instead of the v2 `/apiv2/health/pong`.


## 1.1.0

### Added

- Added `HealthCheckTimeout` (default 800 ms) to `SaksarkivConfiguration`.

### Fixed

- Time-bounded the Saksarkiv health check so a slow/unreachable Saksarkiv reports `Degraded`
  (HTTP 200) quickly instead of blocking consumers' readiness probes.

## 1.0.3

### Changed

- changed(deps): Regenerated the client with Kiota `1.32.4`.

## 1.0.2

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 1.0.1

- Added XML documentation comments for `ISaksarkivTokenProvider` to improve package API documentation.

## 1.0.0

- Initial package with generated Saksarkiv client, configuration, and dependency injection support.
- Bumped `Microsoft.Kiota.Bundle` to `2.0.0` and updated regeneration instructions to the latest Kiota CLI.
