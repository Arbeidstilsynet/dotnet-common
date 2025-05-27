# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 0.0.7

### Changed

- Client returns 'Foo Bar Baz' instead of 'Bar Bar'

## 0.0.6

### Changed

- Add support to release prerelease versions by adding `-alpha`, `-beta` or `-rc` suffixes to the version number.

## 0.0.5

### Changed

- Client returns 'Bar Bar' now instead of 'Bar'.

## 0.0.3

### Added

- New pipeline steps to automatically detect version changes. It ensures that the version and changelog are updated accordingly before merging into main.

## 0.0.2

### Added

- Changelog information.

## 0.0.1

### Added

- First implementation: the client returns 'bar' whenever it is called.
