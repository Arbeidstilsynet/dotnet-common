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
