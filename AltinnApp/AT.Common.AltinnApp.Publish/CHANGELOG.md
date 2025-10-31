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

## 1.4.0

- added: More customization options for LandOptions
  - Custom order function for country list in LandOptions (default is alphabetical by country name)
  - Option for using either alpha2 or alpha3 country codes as value in dropdowns (default is alpha3)

## 1.3.0

### Changed

- changed: Moved all AltinnApp logic (from the Altinn package) to this new dedicated package, only intended to be used by AltinnApps.
