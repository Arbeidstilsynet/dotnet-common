# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added <!-- for new features. -->

- Add `StructuredDataConfiguration.ExcludedAttachmentDataTypeIds` to exclude additional data types from attachments; the datamodel type is included automatically.
- Validate that `MainPdfDataTypeId` and `StructuredDataTypeId` are non-blank, distinct, and never excluded from attachments (including via the datamodel type).

### Changed <!--  for changes in existing functionality. -->

### Deprecated <!--  for soon-to-be removed features. -->

### Removed <!-- for now removed features. -->

### Fixed <!-- for any bug fixes. -->

### Security <!-- in case of vulnerabilities. -->

## 2.12.1

### Fixed

- Synchronize the supplied instance's data collection after successful structured-data insertion and deletion, preserving unrelated elements and avoiding duplicates.
- Verify locally missing structured data against storage once before failing at process end; propagate lookup errors and retain the guard regardless of task filtering or cleanup settings.
- Reuse the fallback refresh for post-processors and preserve fresh framework-created elements, without requiring the form model at process end.
- Reject unsuccessful structured-data deletions without removing their in-memory entries.

### Removed

- **Breaking change:** Remove `KeepAppDataModelAfterMapping` and package-managed form-model deletion. Deletion in `IProcessEnd` could not guarantee cleanup before process-completed event registration. Enable `appLogic.autoDeleteOnProcessEnd` on the existing form-model data type for Altinn-managed cleanup; without it, the form model is retained. See the README migration guide for metadata, test-retention, post-processing, and recovery implications. Process-end validation and post-processors remain supported.

## 2.12.0

### Added

- Add `StructuredDataConfiguration.ExcludedAttachmentDataTypeIds` to exclude additional data types from attachments; the datamodel type is included automatically.
- Validate that `MainPdfDataTypeId` and `StructuredDataTypeId` are non-blank, distinct, and never excluded from attachments (including via the datamodel type).

## 2.11.0

### Added

- feat: Adds `IStructuredDataPostProcessor` and `AddStructuredDataPostProcessor<T>()` for post-processing the instance, structured data element and main PDF element at process end, from inside the structured data feature.

## 2.10.0

### Added

- feat: `IStructuredDataMapper<TDataModel, TStructuredData>` and `AddStructuredData<TDataModel, TStructuredData, TMapper>(env, configure)`. Mappers are resolved from DI (constructor injection, async mapping), and `configure` adjusts the environment-based default `StructuredDataConfiguration`.

### Changed

- Allow publishing with Dialogporten's prerelease client dependency (NU5104).

### Deprecated

- `AddStructuredData<TDataModel, TStructuredData>(Func<TDataModel, TStructuredData>, ...)` overloads. Implement `IStructuredDataMapper` and use the new overload instead.

## 2.9.1

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 2.9.0

### Added

- feat: Adds a new PatchDialog service task and DI extension to enable Altinn apps to patch Dialogporten dialogs, plus supporting ports and dependencies in the Arbeidstilsynet.Common.AltinnApp NuGet package

## 2.8.1

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 2.8.0

### Added

- added: `PreSubmitDataModelProcessor<TDataModel>` supports `TaskIdFilter` for limiting pre-submit processing to selected Altinn task IDs
- added: `StructuredDataConfiguration.TaskIdFilter` for limiting structured data generation to selected Altinn task IDs

### Changed

- changed: `StructuredDataManager` now refuses to delete the app data model if no structured data element of the configured type exists on the instance, guarding against data loss on misconfiguration or when structured data mapping did not run for the task.

## 2.7.2

### Fixed

- fixed: `StructuredDataManager` now deletes any pre-existing `structured-data` data elements before inserting the new one, making `CreateStructuredData` idempotent on retries

## 2.7.1

### Fixed

- fixed: `StructuredDataManager<,>` was registered as singleton but depends (via `IStructuredDataValidator<T>`) on FluentValidation's `IValidator<T>`, which is scoped by default. This caused DI scope validation to fail at startup. The manager is now registered as transient.

## 2.7.0

### Added

- added: `ILanguageObserver` interface and `AddLanguageObserver<T>` DI extension for receiving notifications about the user's selected language during `ProcessDataRead`
- added: `SelectedLanguageProcessor` that resolves the current language from the request or falls back to the user's profile preference. Only notifies when the language is supported by the application
- added: `StructuredDataManager` now validates structured data before insertion. Uses registered `IValidator<T>` implementations, or falls back to `DataAnnotations`. Controlled via `StructuredDataConfiguration.DisableValidation`
- added: `FieldsValidator<TDataModel, TField>` base class for field-level validators with expression-based field paths and change detection
- added: `RepeatingGroupValidator<TDataModel, TItem>` base class for validating items within repeating groups (collections), with indexed path generation
- added: `DataTypeValidator<TDataModel, TToValidate>` base class that automatically discovers and validates all instances of a target type within a data model by walking the object graph via reflection, with circular type reference detection

### Changed

- changed: `ILanguageObserver.NotifyCurrentLanguage` now receives the current data model as a parameter (`object dataModel, string language`)

## 2.6.1

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 2.6.0

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 2.5.0

### Changed

- ListProcessor no longer emits ProcessList -- only ProcessItem, and items are compared using AltinnRowId. If you need to process the list as a whole, you should use MemberProcessor pointing to the list in question.

## 2.4.1

### Changed

- chore: moved package to nuget.org

## 2.4.0

- changed: Differentiate structured data and main content.
  - More configurable, though defaults should for most.
  - The altinn app will declare which data elemnts are the main content, and structured data, using data values

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
