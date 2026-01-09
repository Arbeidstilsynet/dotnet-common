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
