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

## 0.0.1

### Added

- Added implementation of the [GeoNorge API](https://ws.geonorge.no/adresser/v1).
  - `/sok` with pagination
  - `/punktsok` with pagination
- Added extension methods for common access patterns
  - `GetClosestAddress` for getting the closest address returned from `/punktsok`
  - `QuickSearchLocation` for getting the first hit returned from `/sok`
