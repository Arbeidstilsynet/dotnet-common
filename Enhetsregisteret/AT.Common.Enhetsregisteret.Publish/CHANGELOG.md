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

## 1.0.1

### Changed

- changed(deps): Updated internal package referances (remove prerelease version)

## 1.0.0

### Changed

- changed(deps): Major dotnet updated (v10)

## 0.3.0

### Added

- Added support for query parameter `navnMetodeForSoek`
- Added validation for invalid `pagination` (max 10_000 items can be paginated)
- Added validation for search string `navn` length (max 180 characters)

## 0.2.0

### Changed

- fix(deps): use correct version range to only support Microsoft version 8.* packages

## 0.1.2

### Changed

- chore: added brreg staging (preprod) url as default if environment is not production

## 0.1.1

### Changed

- chore(renovate): all non major update

## 0.1.0

### Fixed

- Fixed URL encoding for query parameters by using `Arbeidstilsynet.Common.AspNetCore.Extensions`

### Removed

- Removed the resilience pipeline options for the Enhetsregisteret client. It's just using a standard resilience pipeline now.
- Simplified the cache options.

## 0.0.1

### Added

- Implementation of endpoints for Altinn Enhetsregisteret:
  - `/enheter`
  - `/enheter/{organisasjonsnummer}`
  - `/underenheter/`
  - `/underenheter/{organisasjonsnummer}`
  - `/oppdateringer/enheter`
  - `/oppdateringer/underenheter`

- Extension methods for common usage patterns
