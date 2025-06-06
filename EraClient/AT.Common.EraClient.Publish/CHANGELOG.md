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

## 1.0.0

### Added

- feat: Altinn extensions methods which can be used to reduce code duplication in our altinn projects

### Changed

- refactor: applied updated project structure to project (based on `dotnet new common-package` v1.0.0)

### Fixed

- fix: ERA endpoints require a `User-Agent` header by convention (WAF)
- fix: Updated asbest BaseURL
- fix: Retrieve the base url in runtime, since we are using a "cookie hack" to determine if we should talk to our verifi or valid environment.

## 0.0.1

### Added

- feat: Implemented AuthenticationClient which can be used to retrive an access token by providing client credentials (oauth2). The access token works for all EraClients. 
- feat: Implemented EraAsbestClient to send requests to our internal asbest endpoints. Currently we support to retrieve `meldinger` for an organization as well as the status of an application (søknad).