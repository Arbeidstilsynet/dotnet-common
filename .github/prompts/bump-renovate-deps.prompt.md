---
mode: agent
description: Bump versions and update changelogs for packages affected by Renovate dependency updates
---

# Bump Versions for Renovate Dependency Updates

You are working in a .NET monorepo containing multiple NuGet packages. Renovate has updated dependencies in one or more packages. Your job is to bump the patch version and update the changelog for each affected package.

## Step 1: Identify affected packages

Run `git diff main --name-only` (or `git diff origin/main --name-only`) to find changed files. Filter for files matching the pattern `**/AT.Common.*.Publish/*.csproj`. These are the only packages that need version bumps.

**Exclude** test projects (`AT.Common.*.Test*`) and any other non-publish projects.

If the diff base is not `main`, use the PR's target branch instead.

## Step 2: Bump patch version in each affected .csproj

For each affected `AT.Common.*.Publish/*.csproj` file:

1. Find the `<Version>X.Y.Z</Version>` element in the `<PropertyGroup>`
2. Increment the patch version: `X.Y.Z` → `X.Y.(Z+1)`
3. Do **not** change the major or minor version

Example: `<Version>3.2.3</Version>` → `<Version>3.2.4</Version>`

## Step 3: Update CHANGELOG.md for each affected package

Each affected package has a `CHANGELOG.md` in the same directory as its `.csproj`. Update it by inserting a new version section **immediately after** the `## [Unreleased]` block (after its last subsection comment line).

The `[Unreleased]` section with its subsection comments must remain intact and empty above the new version.

### Changelog format

Insert the following block after the `## [Unreleased]` section (after the `### Security` comment line):

```markdown

## X.Y.Z+1

### Changed

- changed(deps): Applied minor and patch updates to dependencies
```

Where `X.Y.Z+1` is the new bumped version number.

### Example — Before

```markdown
## [Unreleased]

### Added <!-- for new features. -->

### Changed <!--  for changes in existing functionality. -->

### Deprecated <!--  for soon-to-be removed features. -->

### Removed <!-- for now removed features. -->

### Fixed <!-- for any bug fixes. -->

### Security <!-- in case of vulnerabilities. -->

## 3.2.3

### Fixed

- fix(altinn): some fix description
```

### Example — After

```markdown
## [Unreleased]

### Added <!-- for new features. -->

### Changed <!--  for changes in existing functionality. -->

### Deprecated <!--  for soon-to-be removed features. -->

### Removed <!-- for now removed features. -->

### Fixed <!-- for any bug fixes. -->

### Security <!-- in case of vulnerabilities. -->

## 3.2.4

### Changed

- changed(deps): Applied minor and patch updates to dependencies

## 3.2.3

### Fixed

- fix(altinn): some fix description
```

## Step 4: Verify

1. Confirm each bumped version in `.csproj` matches the new changelog section header
2. Confirm the `[Unreleased]` section is still present and unchanged
3. Confirm no test projects or non-publish projects were modified
4. If possible, run `dotnet build` on affected solutions to verify no build errors

## Important rules

- Only bump packages whose `.Publish.csproj` file has actual dependency changes in the diff
- Always bump **patch** version only (dependency updates are non-breaking)
- Never modify the `[Unreleased]` section content — it must stay with its empty subsection comments
- Do not bump a package if it already has the correct next version (avoid double-bumps)
- Do not touch packages that only have test project dependency changes
