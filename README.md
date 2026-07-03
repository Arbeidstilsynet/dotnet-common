# 🌈 dotnet-common

Monorepository for all common (NuGet) packages published by Arbeidstilsynet.

The main purpose of common packages at Arbeidstilsynet is to share code between projects and hence reduce code duplication. Additional advantages of using common packages include:

- **Consistency:** Ensures a uniform approach to solving common problems across multiple projects.
- **Maintainability:** Centralizes updates and bug fixes, making it easier to maintain and improve shared functionality.
- **Faster Development:** Reduces the need to reinvent solutions, allowing teams to focus on project-specific features.
- **Quality Assurance:** Promotes reuse of well-tested components, improving overall code quality.
- **Simplified Dependency Management:** Makes it easier to track and update shared dependencies across projects.

## 🔧 Prerequisites

```cmd
dotnet new install Arbeidstilsynet.Templates
```

or (if already installed):

```cmd
dotnet new update
```

## 📦 Add new package

```cmd
dotnet new common-package -n NewFancyClient
```

After running this command in the root directory, a new `NewFancyClient` directory will appear.
By convention you will get three new projects within this directory:

- AT.Common.NewFancyClient.Publish
- AT.Common.NewFancyClient.Test
- AT.Common.NewFancyClient.Test.ArchUnit

`Publish` should contain all logic which is intended to be exposed via the package. By default, you will get a couple of examples to see how you can use this template.

`Test` is a default dotnet test project, which can be used for testing everything which lays within `Publish`.

`Test.ArchUnit` contains a preset of ArchUnit tests which check our common development conventions. This is useful to maintain a common (project) structure.

## 🚧 Pre-Release

If you want to test your changes by importing them into another project, you can use [Prerelease Packages](https://learn.microsoft.com/en-us/nuget/create-packages/prerelease-packages). To do this, simply update the version number with an `alpha`, `beta`, or `rc` suffix. For example, if you want to release a new version `0.0.2`, a valid prerelease version would be `0.0.2-alpha`. You can find the current version number in `AT.Common.NewFancyClient.Publish.csproj`.

## 🚀 Publish
Create a new branch and pull request. Remember to increment the version in `AT.Common.NewFancyClient.Adapters.csproj`. When the pull request is merged, a new release pipeline will start automatically.

## 🛠️ Update Dependencies

Renovate is configured to group all non-major versions together. Check Renovate's PR, update the version and changelog for the affected packages according to the updates, then commit & merge.

## 🔄 Refreshing generated clients

Some packages ship a [Kiota](https://learn.microsoft.com/openapi/kiota/)-generated client produced from an OpenAPI specification. When the upstream API publishes a new spec, the generated code needs to be regenerated.

Kiota is installed as a **local dotnet tool** (pinned in [`.config/dotnet-tools.json`](.config/dotnet-tools.json)), so every contributor regenerates with the exact same version and there is no global install to manage. Each package with a generated client exposes a `generate:client` npm script that wraps the correct Kiota command.

Packages that use this workflow:

| Package | Spec file | npm script |
| --- | --- | --- |
| `Enhetsregisteret` | `AT.Common.Enhetsregisteret.Publish/openapi.json` | `generate:client` |
| `GeoNorge` | `AT.Common.GeoNorge.Publish/openapi-adresser.json`, `AT.Common.GeoNorge.Publish/openapi-kommuneinfo.json` | `generate:client` |
| `Saksarkiv` | `AT.Common.Saksarkiv.Publish/openApi.json` | `generate:client` |

### Steps

1. Replace the package's OpenAPI spec (`openapi.json` / `openApi.json`) with the new version from the upstream API. Some packages (e.g. `GeoNorge`) ship more than one spec — replace each one.
2. Restore the local tools (only needed once per checkout, or after the pinned Kiota version changes):

   ```bash
   dotnet tool restore
   ```

3. Regenerate the client from the package directory:

   ```bash
   cd Enhetsregisteret   # or: cd GeoNorge / cd Saksarkiv
   npm run generate:client
   ```

   Packages with multiple specs regenerate every client from the single `generate:client` script (`GeoNorge` also exposes `generate:client:adresser` and `generate:client:kommuneinfo` if you need to regenerate just one).

4. Review the diff in the `Generated/` folder and run the package tests:

   ```bash
   dotnet test Enhetsregisteret.sln   # or: dotnet test GeoNorge.sln / dotnet test Saksarkiv.sln
   ```

### Notes

- The generated code (including `Generated/kiota-lock.json`) is checked in and should be committed together with the spec change.
- To bump the Kiota version for all packages, update `microsoft.openapi.kiota` in `.config/dotnet-tools.json`, run `dotnet tool restore`, then regenerate every client so the checked-in output stays in sync.
- If the OpenAPI contract changes the path structure used by consumers, review the fluent API call sites (and any local adapters) after regeneration.
