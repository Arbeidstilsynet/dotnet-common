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

## 1.2.1

### Added

- feat(extensions): added enum to string converter as default behavior when adding controllers
- feat(extensions): added custom controller detector to also detect internal controllers (before it detected only public classes)

## 1.2.0

### Added

- feat(extensions): added extension method for adding query parameters to a Uri (`AddQueryParameters`)

## 1.1.2

### Changed

- chore(deps): update dependency swashbuckle.aspnetcore to v8

## 1.1.1

### Changed

- chore(deps): update dependency scalar.aspnetcore to v2

## 1.1.0

### Added

- Adds extension method ConfigureCors to StartupExtensions

## 1.0.0

### Changed

- refactor: applied updated project structure to project (based on `dotnet new common-package` v1.0.0)

### 0.0.3

### Removed

- Removed the ConfigureLogging method from StartupExtensions, as it was just wrapping services.AddLogging().

## 0.0.2

### Changed

- Made the extension methods more granular, instead of having a single `ConfigureApi` method, we now have separate methods for OpenTelemetry, Swagger, Logging, and Exception Handling.

## 0.0.1

Adds extension methods to consolidate ASP.NET configuration and new helpers for IConfiguration.

Cross cutting concerns are handled in a single place, including API setup,

- OpenTelemetry
- Swagger
- Logging
- Model validation
- Exception handling (mapping exceptions to HTTP responses)
- Scalar

### Added

- Introduces StartupExtensions with methods for API setup, OpenTelemetry, Swagger, logging, exception handling, and Scalar endpoints.
- Adds ConfigurationExtensions to bind and validate configuration sections with detailed error reporting.
- Supplies unit tests for service name conversion, configuration binding, exception mapping, and architecture rules; includes initial README and changelog entries.
