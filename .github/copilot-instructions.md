# Repository Instructions for Copilot

## Repository Structure

This is a .NET monorepo containing multiple independently-versioned NuGet packages. Each package lives in its own top-level folder with the following structure:

```
PackageName/
├── AT.Common.PackageName.Publish/     # The published NuGet package
│   ├── AT.Common.PackageName.Publish.csproj
│   ├── CHANGELOG.md
│   └── README.md
├── AT.Common.PackageName.Test/        # Unit tests (not published)
└── AT.Common.PackageName.Test.ArchUnit/  # Architecture tests (not published)
```

Current packages: Altinn, AltinnApp, AspNetCore, BlubExtensions, Enhetsregisteret, EraClient, FeatureFlags, GeoNorge, Saksarkiv, TestExtensions.

## Versioning Conventions

- Each package has its own independent version in `<Version>X.Y.Z</Version>` inside its `.Publish.csproj` file.
- Versions follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
- **Patch bump** for dependency updates, bug fixes, and non-breaking changes.
- **Minor bump** for new features (backwards-compatible).
- **Major bump** for breaking changes.

## Changelog Conventions

Each published package has a `CHANGELOG.md` following [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) format:

- An `## [Unreleased]` section at the top with subsection comments (Added, Changed, Deprecated, Removed, Fixed, Security).
- Each release gets a `## X.Y.Z` section below `[Unreleased]`.
- The `[Unreleased]` section must always remain with its empty subsection placeholders.

## Renovate Dependency Update Task

When assigned to a Renovate PR that updates dependencies:

1. Run `git diff origin/main --name-only` to find which `AT.Common.*.Publish/*.csproj` files have changes.
2. For each affected `.Publish.csproj`:
   - Increment the patch version: `<Version>X.Y.Z</Version>` → `<Version>X.Y.(Z+1)</Version>`
3. For each affected package's `CHANGELOG.md`:
   - Insert a new version section after the `[Unreleased]` block (after the `### Security` comment line):
     ```
     ## X.Y.Z+1

     ### Changed

     - changed(deps): Applied minor and patch updates to dependencies
     ```
4. Do **not** modify test projects, the `[Unreleased]` section, or packages without direct dependency changes.
5. Verify the build passes: `dotnet build` on affected solutions.

## Build & Test

- .NET SDK version is specified in `global.json` (currently 10.0.301).
- Each package folder has a `.sln` file at its root (e.g., `Altinn/Altinn.sln`).
- Build a specific solution: `dotnet build Altinn/Altinn.sln`
- Run tests: `dotnet test Altinn/Altinn.sln`
- Build all: find all `.sln` files and build each.

## NuGet Publishing

- Publishing is handled by GitHub Actions (`.github/workflows/publish-template.yml`).
- The workflow extracts version from `.csproj`, checks if already published on NuGet, and creates a GitHub Release with the changelog section as the body.
- Release tag format: `PackageName@Version` (e.g., `Arbeidstilsynet.Common.Altinn@3.2.4`)
