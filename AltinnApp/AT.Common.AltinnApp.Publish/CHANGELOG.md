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

## 2.3.0

- added: PreSubmitProcessor to make it easier to implement custom pre-submit logic in AltinnApps.
- added: A few more extension methods for `IApplicationClient` and `IDataClient`

## 2.2.0

- added: Option to disable deletion of app datamodel after mapping structured data. This can be useful for testing and some legacy use cases.

## 2.1.0

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 2.0.0

### Changed

- changed(deps): Major dotnet updated (v10)

## 1.5.1

- added: Option to include error details related to mapping structured data in the validation response. This intended for development environments.

## 1.5.0

- added: Extension to manage DataElements for structured data. It deletes the xml-based DataModel, and replaces it with application/json of a user-provided type.

## 1.4.1

### Changed

- applied new default skjema type `structured-data` as default

## 1.4.0

- added: More customization options for LandOptions
  - Custom order function for country list in LandOptions (default is alphabetical by country name)
  - Option for using either alpha2 or alpha3 country codes as value in dropdowns (default is alpha3)

## 1.3.0

### Changed

- changed: Moved all AltinnApp logic (from the Altinn package) to this new dedicated package, only intended to be used by AltinnApps.
