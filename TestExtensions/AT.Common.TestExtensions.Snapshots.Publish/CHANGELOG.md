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

## 0.1.0-preview.1

### Added

- feat: `Snapshot.Verify` and `Snapshot.Serialize`. These use a snapshot format compatible with Verify, the received/verified workflow and `SNAPSHOT_ACCEPT` / `UPDATE_SNAPSHOTS` accept mode.
- feat: `SnapshotSettings` with directory, file name, parameter, GUID and date scrubbing options and custom scrubbers.
- feat: `Snapshot.VerifyTargets` for multi-file snapshots, used by `Arbeidstilsynet.Common.TestExtensions.Snapshots.Pdf`.
