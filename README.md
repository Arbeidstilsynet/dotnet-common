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

## Commands to run before PR

dotnet format

dotnet csharpier format .
dotnet csharpier check .
