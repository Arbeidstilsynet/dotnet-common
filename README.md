# 🌈 dotnet-common

Monorepository for all common (nuget) packages published by Arbeidstilsynet.

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
