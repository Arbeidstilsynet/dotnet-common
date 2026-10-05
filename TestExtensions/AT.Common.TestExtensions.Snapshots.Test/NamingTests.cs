using Arbeidstilsynet.Common.TestExtensions.Snapshots.Test.Golden;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Test;

/// <summary>
/// Mirrors the naming cases run against Verify. The files in <c>Naming/</c> were produced by Verify,
/// so these tests pass only when the file names are resolved identically.
/// </summary>
public class NamingTests
{
    private readonly SnapshotSettings _settings = new SnapshotSettings().UseDirectory("Naming");

    [Fact]
    public Task Fact_Simple() => Snapshot.Verify(1, _settings);

    [Theory]
    [InlineData("Bokmål")]
    [InlineData("a/b c")]
    [InlineData("")]
    [InlineData(null)]
    public Task Theory_String(string? språk)
    {
        _ = språk;
        return Snapshot.Verify(1, _settings);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(-2, false)]
    public Task Theory_IntBool(int n, bool flag)
    {
        _ = (n, flag);
        return Snapshot.Verify(1, _settings);
    }

    [Theory]
    [InlineData(Color.Hovedentreprenør)]
    public Task Theory_Enum(Color c)
    {
        _ = c;
        return Snapshot.Verify(1, _settings);
    }

    [Theory]
    [InlineData(1.5, 'x')]
    public Task Theory_DoubleChar(double d, char ch)
    {
        _ = (d, ch);
        return Snapshot.Verify(1, _settings);
    }

    [Theory]
    [InlineData(new[] { 1, 2 })]
    public Task Theory_Array(int[] arr)
    {
        _ = arr;
        return Snapshot.Verify(1, _settings);
    }

    [Theory]
    [InlineData("3aa1d561-8c4e-7975-9c9b-f9dff6ede8e5")]
    public Task Theory_GuidString(string g)
    {
        _ = g;
        return Snapshot.Verify(1, _settings);
    }

    [Fact]
    public async Task Fact_Async_Suffix_Async()
    {
        await Task.Yield();
        await Snapshot.Verify(1, _settings);
    }

    [Fact]
    public Task Fact_ExplicitParams() => Snapshot.Verify(1, _settings.UseParameters("x", 2));

    [Theory]
    [InlineData(1, 2)]
    public Task Theory_ExplicitSubset(int a, int b)
    {
        _ = b;
        return Snapshot.Verify(1, _settings.UseParameters(a));
    }

    public class NestedClass
    {
        [Fact]
        public Task InNested() => Snapshot.Verify(1, new SnapshotSettings().UseDirectory("Naming"));
    }
}
