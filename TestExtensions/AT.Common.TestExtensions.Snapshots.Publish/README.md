# Arbeidstilsynet.Common.TestExtensions.Snapshots

Minimal snapshot testing for .NET and xunit v3.

The library serializes a value to readable text and compares it with a committed `*.verified.txt` file. On a mismatch it writes a `*.received.txt` file next to it and fails the test with a diff.

The snapshot format is **compatible with Verify's snapshot format**, so existing `.verified.txt` files can usually be reused unchanged. The library is a clean-room implementation and contains no Verify code. The full format and behaviour are specified in [SPEC.md](https://github.com/Arbeidstilsynet/dotnet-common/blob/main/TestExtensions/AT.Common.TestExtensions.Snapshots.Publish/SPEC.md).

## Usage

```csharp
using Arbeidstilsynet.Common.TestExtensions.Snapshots;

public class SakRepositoryTests
{
    [Fact]
    public async Task Create_StoresSak()
    {
        var sak = await _repository.Create(...);

        await Snapshot.Verify(sak, new SnapshotSettings().UseDirectory("Snapshots"));
    }
}
```

This creates `Snapshots/SakRepositoryTests.Create_StoresSak.verified.txt`:

```
{
  Id: Guid_1,
  Opprettet: DateTimeOffset_1,
  Status: Mottatt
}
```

`Snapshot.Serialize(value, settings)` returns the text without comparing it.

## Settings

| Method | Effect |
| --- | --- |
| `UseDirectory(path)` | Store snapshots in `path`. A relative path is resolved from the test source file's directory. |
| `UseFileName(name)` | Override `{Class}.{Method}{Parameters}`. |
| `UseParameters(values)` | Override the theory arguments used in the file name. |
| `ScrubInlineGuids()` | Replace GUIDs inside longer strings with `Guid_n`. |
| `DontScrubGuids()` | Write `Guid` values as-is. |
| `DontScrubDateTimes()` | Write `DateTime`, `DateTimeOffset`, `DateOnly` and ISO date strings as-is. |
| `DontIgnoreEmptyCollections()` | Write empty collections as `[]` instead of omitting them. |
| `IncludeDefaultValues()` | Write `null` and default members. |
| `AddScrubber(Func<string, string>)` | Post-process the serialized text. |

Theory arguments from xunit v3's `TestContext` become part of the file name, e.g. `Tests.Method_language=Bokmål.verified.txt`.

## Workflow

1. Run the tests. New or changed snapshots fail and leave `*.received.*` files.
2. Review the diff in the failure message (or diff the files in your IDE).
3. Accept the changes in one of these ways:
   - Rename `*.received.*` to `*.verified.*`, or run `scripts/accept-snapshots.sh [directory]` from the dotnet-common repository.
   - Re-run with `SNAPSHOT_ACCEPT=1` (or `UPDATE_SNAPSHOTS=1`). The verified files are overwritten and the tests pass. Accept mode is ignored when `CI=true`.
4. Commit the `*.verified.*` files. Add `*.received.*` to `.gitignore`.

Snapshot files are UTF-8 with BOM and `\n` line endings. Consider adding this to `.gitattributes`:

```
*.verified.txt text eol=lf working-tree-encoding=UTF-8
*.verified.png binary
```

## Migrating from Verify

| Verify | Snapshots |
| --- | --- |
| `await Verify(value)` | `await Snapshot.Verify(value)` |
| `await Verify(value, settings)` | `await Snapshot.Verify(value, settings)` |
| `new VerifySettings()` | `new SnapshotSettings()` |
| `settings.UseDirectory(...)`, `ScrubInlineGuids()`, `DontScrubGuids()`, `DontScrubDateTimes()`, `DontIgnoreEmptyCollections()`, `UseParameters(...)` | Same names |
| `.UseParameters(x)` on the `Verify(...)` call | `settings.UseParameters(x)` (xunit v3 theory arguments are picked up automatically) |
| `AddScrubber(StringBuilder => ...)` | `AddScrubber(string => string)` |

Deliberate differences are listed in [SPEC.md §5](https://github.com/Arbeidstilsynet/dotnet-common/blob/main/TestExtensions/AT.Common.TestExtensions.Snapshots.Publish/SPEC.md#5-known-deliberate-deviations-from-verify).

## Requirements

- .NET 10
- xunit v3 for test-name resolution. Other frameworks work with `UseFileName`, or fall back to the calling file and member name.
