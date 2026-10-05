using System.Collections.ObjectModel;
using System.Security.Claims;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Test.Golden;

[Flags]
public enum GoldenOptions
{
    None = 0,
    DontScrubGuids = 1,
    DontScrubDateTimes = 2,
    DontIgnoreEmptyCollections = 4,
    IncludeDefaultValues = 8,
    ScrubInlineGuids = 16,
}

public sealed record GoldenCase(
    string Name,
    Func<object?> Create,
    GoldenOptions Options = GoldenOptions.None,
    string[]? ScrubMembers = null
)
{
    public override string ToString() => Name;
}

public enum Color
{
    Red,
    Green,
    Blue,
    Hovedentreprenør,
}

[Flags]
public enum Perms
{
    None = 0,
    Read = 1,
    Write = 2,
    Exec = 4,
}

public class Primitives
{
    public int Int { get; set; } = 42;
    public int NegInt { get; set; } = -7;
    public long Long { get; set; } = 85179772002;
    public short Short { get; set; } = 3;
    public byte Byte { get; set; } = 255;
    public uint UInt { get; set; } = 4000000000;
    public ulong ULong { get; set; } = 18446744073709551615;
    public double DoubleWhole { get; set; } = 1.0;
    public double DoubleFrac { get; set; } = 4.1797;
    public double DoubleNeg { get; set; } = -90.4015;
    public double DoubleBig { get; set; } = 1e20;
    public double DoubleSmall { get; set; } = 1e-7;
    public double DoubleThird { get; set; } = 1.0 / 3.0;
    public float FloatFrac { get; set; } = 1.5f;
    public float FloatWhole { get; set; } = 2f;
    public decimal DecimalWhole { get; set; } = 1.0m;
    public decimal DecimalScaled { get; set; } = 12.50m;
    public decimal DecimalInt { get; set; } = 100m;
    public bool True { get; set; } = true;
    public char Char { get; set; } = 'x';
    public Color Enum { get; set; } = Color.Blue;
    public Color EnumUnicode { get; set; } = Color.Hovedentreprenør;
    public Perms Flags { get; set; } = Perms.Read | Perms.Exec;
    public Perms FlagsUndefined { get; set; } = (Perms)64;
    public Uri Uri { get; set; } = new("https://example.com/a?b=c");
    public Version Version { get; set; } = new(1, 2, 3);
    public TimeSpan TimeSpan { get; set; } = new(1, 2, 3, 4, 5);
    public TimeOnly TimeOnly { get; set; } = new(13, 45, 10);
}

public class Defaults
{
    public int Int { get; set; }
    public double Double { get; set; }
    public bool False { get; set; }
    public string? NullString { get; set; }
    public string EmptyString { get; set; } = "";
    public Guid EmptyGuid { get; set; }
    public DateTime MinDate { get; set; }
    public Color DefaultEnum { get; set; }
    public int? NullInt { get; set; }
    public int? ZeroNullableInt { get; set; } = 0;
    public bool? FalseNullableBool { get; set; } = false;
    public Guid? NullGuid { get; set; }
    public List<int> EmptyList { get; set; } = [];
    public List<int>? NullList { get; set; }
    public Dictionary<string, int> EmptyDict { get; set; } = new();
    public int[] EmptyArray { get; set; } = [];
    public Child? NullChild { get; set; }
    public Child EmptyChild { get; set; } = new();
    public char DefaultChar { get; set; }
    public decimal Decimal { get; set; }
}

public class Child
{
    public string? Name { get; set; }
    public int Value { get; set; }
    public List<string>? Tags { get; set; }
}

public class Strings
{
    public string Plain { get; set; } = "Samuel Haley";
    public string Number { get; set; } = "03";
    public string Quotes { get; set; } = "say \"hi\" and 'bye'";
    public string Punctuation { get; set; } = "a, b: c; {d} [e]";
    public string Backslash { get; set; } = @"C:\temp\file.txt";
    public string Tab { get; set; } = "a\tb";
    public string Leading { get; set; } = "  padded  ";
    public string Whitespace { get; set; } = "   ";
    public string Unicode { get; set; } = "Ærlig østers på Å — ✓";
    public string MultiLine { get; set; } = "line1\nline2\nline3";
    public string MultiLineCrLf { get; set; } = "line1\r\nline2";
    public string TrailingNewline { get; set; } = "line1\n";
    public string LiteralNull { get; set; } = "null";
    public string LooksTrue { get; set; } = "true";
    public string Url { get; set; } = "https://example.com/path?x=1&y=2";
    public string Html { get; set; } = "<b>bold</b> & more";
    public string ControlChar { get; set; } = "bell\u0007end";
}

public class Times
{
    public DateTime Utc { get; set; } =
        new DateTime(2026, 2, 28, 7, 46, 15, DateTimeKind.Utc).AddTicks(3770601);
    public DateTime UtcMidnight { get; set; } = new(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
    public DateTime UtcSeconds { get; set; } = new(2026, 6, 10, 12, 30, 5, DateTimeKind.Utc);
    public DateTime UtcMillis { get; set; } = new(2026, 6, 10, 12, 30, 5, 123, DateTimeKind.Utc);
    public DateTime EndOfDay { get; set; } =
        new DateTime(2026, 7, 9, 0, 0, 0, DateTimeKind.Utc).AddTicks(-1);
    public DateTime Unspecified { get; set; } = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified);
    public DateTime UnspecifiedMidnight { get; set; } =
        new(2026, 1, 2, 0, 0, 0, DateTimeKind.Unspecified);
    public DateTimeOffset OffsetZero { get; set; } = new(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
    public DateTimeOffset OffsetZeroTime { get; set; } = new(2026, 3, 1, 10, 20, 30, TimeSpan.Zero);
    public DateTimeOffset OffsetPlusTwo { get; set; } =
        new(2026, 3, 1, 10, 20, 30, TimeSpan.FromHours(2));
    public DateTimeOffset OffsetMinus { get; set; } =
        new(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(-5.5));
    public DateOnly Date { get; set; } = new(1985, 7, 17);
    public DateTime? NullableDate { get; set; } = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public DateTime DuplicateOfUtc { get; set; } =
        new DateTime(2026, 2, 28, 7, 46, 15, DateTimeKind.Utc).AddTicks(3770601);
    public DateOnly DuplicateDate { get; set; } = new(1985, 7, 17);
    public DateOnly OtherDate { get; set; } = new(1990, 1, 1);
    public DateTime MinValue { get; set; } = DateTime.MinValue;
    public DateTime MaxValue { get; set; } = DateTime.MaxValue;
    public DateOnly DateMin { get; set; } = DateOnly.MinValue;
    public string DateLikeString { get; set; } = "2026-02-28T07:46:15Z";
    public string DateInSentence { get; set; } = "Sent 2026-02-28 07:46:15 by user";
    public List<DateTime> List { get; set; } =
    [new(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), new(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)];
}

public class WholeMinutes
{
    public DateTime UtcMinutes { get; set; } = new(2025, 9, 25, 12, 0, 0, DateTimeKind.Utc);
    public DateTime UtcMinutesOdd { get; set; } = new(2025, 9, 25, 13, 7, 0, DateTimeKind.Utc);
    public DateTime UnspecifiedMinutes { get; set; } =
        new(2026, 1, 2, 3, 4, 0, DateTimeKind.Unspecified);
    public DateTimeOffset OffsetMinutes { get; set; } =
        new(2026, 3, 1, 10, 20, 0, TimeSpan.FromHours(2));
    public DateTimeOffset OffsetMinutesHalf { get; set; } =
        new(2026, 3, 1, 10, 20, 0, TimeSpan.FromMinutes(330));
}

public class ClaimHolder
{
    public Claim Single { get; set; } = new("scope", "test:read");
    public List<Claim> Claims { get; set; } =
    [
        new("aud", "https://example.com"),
        new(ClaimTypes.Name, "Ola"),
        new("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "admin"),
        new("http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor", "x"),
        new("exp", "1791204808", ClaimValueTypes.Integer64),
        new("empty", ""),
        new("jti", "3aa1d561-8c4e-7975-9c9b-f9dff6ede8e5"),
        WithProperties(),
    ];

    private static Claim WithProperties()
    {
        var claim = new Claim("p", "v");
        claim.Properties["b"] = "2";
        claim.Properties["a"] = "1";
        return claim;
    }
}

public class ScrubTarget
{
    public string Secret { get; set; } = "s";
    public string? NullSecret { get; set; }
    public int ZeroSecret { get; set; }
    public DateTime When { get; set; } = new(2026, 1, 1, 1, 1, 1, DateTimeKind.Utc);
    public string Keep { get; set; } = "k";
    public ScrubChild Child { get; set; } = new();
    public Dictionary<string, string> Dict { get; set; } =
        new() { ["Secret"] = "d", ["other"] = "o" };
    public List<Claim> Claims { get; set; } = [new("Secret", "c"), new("Keep", "k")];
}

public class ScrubChild
{
    public string Secret { get; set; } = "nested";
    public List<int> Numbers { get; set; } = [1];
}

public class ScrubCollection
{
    public List<int> Secret { get; set; } = [1, 2];
    public ScrubChild When { get; set; } = new();
}

public class Guids
{
    public static readonly Guid A = Guid.Parse("3aa1d561-8c4e-7975-9c9b-f9dff6ede8e5");
    public static readonly Guid B = Guid.Parse("a75da64f-053a-4c1e-9a1b-111122223333");

    public Guid First { get; set; } = A;
    public Guid Second { get; set; } = B;
    public Guid Again { get; set; } = A;
    public Guid Empty { get; set; } = Guid.Empty;
    public Guid? Nullable { get; set; } = B;
    public string GuidString { get; set; } = A.ToString();
    public string GuidUpper { get; set; } = B.ToString().ToUpperInvariant();
    public string GuidBraces { get; set; } = "{" + A + "}";
    public string GuidN { get; set; } = A.ToString("N");
    public string InlineGuid { get; set; } = $"{A}-jp-inngaaende";
    public string SentenceGuid { get; set; } = $"Ref {B} was created";
    public string NewGuidInline { get; set; } = "id 11111111-2222-3333-4444-555555555555 end";
    public List<Guid> List { get; set; } =
    [B, A, Guid.Parse("11111111-2222-3333-4444-555555555555")];
    public Dictionary<Guid, string> Keyed { get; set; } = new() { [A] = "a" };
}

public class Nested
{
    public Child Child { get; set; } =
        new()
        {
            Name = "c",
            Value = 1,
            Tags = ["x", "y"],
        };
    public List<Child> Children { get; set; } = [new() { Name = "a" }, new() { Value = 2 }];
    public List<List<int>> Matrix { get; set; } =
    [
        [1, 2],
        [3],
        [],
    ];
    public string?[] WithNulls { get; set; } = ["a", null, "b"];
    public List<Child?> ChildrenWithNull { get; set; } = [new() { Name = "z" }, null];
    public object Boxed { get; set; } = 5;
    public object Runtime { get; set; } = new Child { Name = "runtime" };
    public IEnumerable<int> Linq { get; set; } = Enumerable.Range(1, 3).Select(x => x * 2);
    public HashSet<string> Set { get; set; } = ["b", "a"];
    public ReadOnlyCollection<int> ReadOnly { get; set; } = new([9, 8]);
    public (int, string) Tuple { get; set; } = (1, "one");
    public KeyValuePair<string, int> Pair { get; set; } = new("k", 1);
    public byte[] Bytes { get; set; } = "hoveddokument"u8.ToArray();
    public byte[] EmptyBytes { get; set; } = [];
    public Child[] EmptyChildren { get; set; } = [];
    public List<Child> ChildrenOfEmpty { get; set; } = [new()];
}

public class Dictionaries
{
    public Dictionary<string, object> Mixed { get; set; } =
        new()
        {
            { "string-field", "fugiat" },
            { "int-field", 1 },
            { "long-field", 2L },
            { "double-field", 1.0 },
            { "bool-field", false },
            { "date-field", new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero) },
            { "datetime-field", new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc) },
            {
                "nested-field",
                new Dictionary<string, object> { { "nested-string-field", "ut" } }
            },
            { "Zeta", "upper" },
            { "alpha", "lower" },
            {
                "list-field",
                new List<object> { 1, "two" }
            },
        };
    public Dictionary<int, string> IntKeys { get; set; } = new() { [10] = "ten", [2] = "two" };
    public Dictionary<string, int> OddKeys { get; set; } =
        new()
        {
            ["a b"] = 1,
            ["a:b"] = 2,
            [""] = 3,
            ["Ø"] = 4,
        };
    public Dictionary<Color, int> EnumKeys { get; set; } =
        new() { [Color.Green] = 1, [Color.Red] = 0 };
    public Dictionary<string, Child> ObjectValues { get; set; } =
        new() { ["c"] = new() { Name = "n" } };
    public Dictionary<string, string?> NullValue { get; set; } =
        new() { ["n"] = null, ["v"] = "x" };
    public Dictionary<string, int> ZeroValue { get; set; } = new() { ["zero"] = 0 };
    public SortedDictionary<string, int> Sorted { get; set; } = new() { ["b"] = 2, ["a"] = 1 };
    public IReadOnlyDictionary<string, int> ReadOnly { get; set; } =
        new Dictionary<string, int> { ["r"] = 1 };
}

public record PositionalRecord(string Name, int Age, Child? Child = null);

public record BaseRecord
{
    public string BaseProp { get; init; } = "base";
    public int Shared { get; init; } = 1;
}

public record DerivedRecord : BaseRecord
{
    public string DerivedProp { get; init; } = "derived";
}

public class WithFields
{
    public string PublicField = "field";
    public readonly int ReadonlyField = 3;
    public static string StaticProp { get; set; } = "static";
    public const string Const = "const";
    private string Private { get; set; } = "private";
    internal string Internal { get; set; } = "internal";
    public string WriteOnly
    {
        set { }
    }
    public string Prop { get; set; } = "prop";
    public string Computed => Prop + "!";
    public string this[int i] => "indexer";

    public string GetPrivate() => Private;
}

public class Throwing
{
    public string Ok { get; set; } = "ok";
    public string Boom => throw new InvalidOperationException("boom");
}

public class Node
{
    public string Name { get; set; } = "";
    public Node? Parent { get; set; }
    public List<Node> Children { get; set; } = [];
}

public class ObjectHolder
{
    public object? Value { get; set; }
}

public class StringDates
{
    public string IsoZ { get; set; } = "2026-02-28T07:46:15Z";
    public string IsoNoZone { get; set; } = "2026-02-28T07:46:15";
    public string IsoFraction { get; set; } = "2026-02-28T07:46:15.1234567Z";
    public string IsoOffset { get; set; } = "2026-02-28T07:46:15+02:00";
    public string DateOnlyString { get; set; } = "2026-02-28";
    public string SpaceSeparated { get; set; } = "2026-02-28 07:46:15";
    public string Digit { get; set; } = "1";
    public string Year { get; set; } = "2026";
    public string Clock { get; set; } = "10:20";
    public string Words { get; set; } = "March 1, 2026";
    public string IsoZAgain { get; set; } = "2026-02-28T07:46:15Z";
    public DateTimeOffset SameInstant { get; set; } = new(2026, 2, 28, 7, 46, 15, TimeSpan.Zero);
}

public class Offsets
{
    public DateTimeOffset PlusFiveThirty { get; set; } =
        new(2026, 3, 1, 1, 2, 3, TimeSpan.FromMinutes(330));
    public DateTimeOffset PlusTen { get; set; } = new(2026, 3, 1, 1, 2, 3, TimeSpan.FromHours(10));
    public DateTimeOffset MinusTwo { get; set; } = new(2026, 3, 1, 1, 2, 3, TimeSpan.FromHours(-2));
    public DateTimeOffset PlusHalf { get; set; } =
        new(2026, 3, 1, 1, 2, 3, TimeSpan.FromMinutes(30));
    public DateTimeOffset WithFraction { get; set; } =
        new DateTimeOffset(2026, 3, 1, 1, 2, 3, TimeSpan.Zero).AddTicks(1230000);
    public DateTimeOffset Min { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset Max { get; set; } = DateTimeOffset.MaxValue;
    public DateTime Local { get; set; } = new(2026, 3, 1, 1, 2, 3, DateTimeKind.Local);
    public DateOnly DateMax { get; set; } = DateOnly.MaxValue;
    public TimeOnly Time { get; set; } = new(13, 45, 10);
    public TimeOnly TimeFraction { get; set; } =
        new TimeOnly(13, 45, 10).Add(TimeSpan.FromTicks(5));
    public TimeOnly TimeMidnight { get; set; } = TimeOnly.MinValue;
    public TimeOnly TimeMax { get; set; } = TimeOnly.MaxValue;
    public TimeOnly TimeAgain { get; set; } = new(13, 45, 10);
    public DateOnly? NullableDate { get; set; } = new(2000, 1, 1);
    public Color? NullableEnum { get; set; } = Color.Red;
}

public class MemberOrderBase
{
    public string BaseProp { get; set; } = "bp";
    public string BaseField = "bf";
}

public class MemberOrder : MemberOrderBase
{
    public string A { get; set; } = "a";
    public string B = "b";
    public string C { get; set; } = "c";
    public new string BaseProp { get; set; } = "hidden";
}

public class Multiline
{
    public List<string> List { get; set; } = ["a\nb", "c", "", " "];
    public Child Nested { get; set; } = new() { Name = "x\ny" };
    public Dictionary<string, string> Dict { get; set; } = new() { ["k"] = "1\n2", ["l"] = "z" };
    public string Last { get; set; } = "end1\nend2";
}

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class Structs
{
    public Point Point { get; set; } = new() { X = 1, Y = 0 };
    public Point DefaultPoint { get; set; }
    public Point? NullablePoint { get; set; } = new() { X = 2 };
    public List<KeyValuePair<string, int>> Pairs { get; set; } = [new("b", 1), new("a", 2)];
    public Dictionary<string, Guid> GuidValues { get; set; } =
        new() { ["b"] = Guids.A, ["a"] = Guids.B };
}

public class MultilineScrub
{
    public string GuidLine { get; set; } = $"a\n{Guids.A}\nb";
    public string GuidLineFirst { get; set; } = $"{Guids.B}\nb";
    public string GuidLineLast { get; set; } = $"a\n{Guids.A}";
    public string GuidLineSpaces { get; set; } = $"a\n {Guids.B} \nb";
    public string GuidInLine { get; set; } = $"a\nref {Guids.B} x\nb";
    public string GuidNLine { get; set; } = $"a\n{Guids.A:N}\nb";
    public string GuidCrLf { get; set; } = $"a\r\n{Guids.B}\r\nb";
    public string GuidTrailing { get; set; } = $"{Guids.A}\n";
    public string NewGuidLine { get; set; } = "x\n11111111-2222-3333-4444-555555555555\ny";
    public string DateLine { get; set; } = "a\n2026-02-28T07:46:15Z\nb";
    public string DateSpaceLine { get; set; } = "a\n2026-02-28 07:46:15\nb";
    public string DateOnlyLine { get; set; } = "a\n2026-02-28\nb";
    public string DateInLine { get; set; } = "a\nat 2026-02-28T07:46:15Z ok\nb";
    public List<string> List { get; set; } = [$"q\n{Guids.A}"];
}

public class GuidProbe
{
    public string Spaced { get; set; } = $" {Guids.A} ";
    public string Tabbed { get; set; } = $"\t{Guids.B}";
    public string XFormat { get; set; } = Guids.A.ToString("X");
    public string PFormat { get; set; } = Guids.B.ToString("P");
    public string NInline { get; set; } = $"x {Guids.A:N} y";
    public string NAdjacent { get; set; } = $"x{Guids.B:N}y";
    public string BInline { get; set; } = $"x {Guids.A:B} y";
    public string PInline { get; set; } = $"x {Guids.B:P} y";
    public string UpperInline { get; set; } = $"x {Guids.B.ToString().ToUpperInvariant()} y";
    public string Underscore { get; set; } = $"x_{Guids.A}_y";
    public string DigitAdjacent { get; set; } = $"x {Guids.A}0 y";
    public string LetterAdjacent { get; set; } = $"x {Guids.A}g y";
    public string Hex33 { get; set; } = "x 0123456789abcdef0123456789abcdef0 y";
    public string Hex32 { get; set; } = "x 0123456789abcdef0123456789abcdef y";
    public string TwoInline { get; set; } = $"{Guids.A}{Guids.B}";
    public string Dotted { get; set; } = $"file.{Guids.A}.pdf";
}

public static class GoldenCorpus
{
    public static IReadOnlyList<GoldenCase> Cases { get; } = Build();

    private static List<GoldenCase> Build()
    {
        var all = GoldenOptions.DontScrubGuids | GoldenOptions.DontScrubDateTimes;
        return
        [
            new("WholeMinutes", () => new WholeMinutes()),
            new("WholeMinutesUnscrubbed", () => new WholeMinutes(), GoldenOptions.DontScrubDateTimes),
            new("Claims", () => new ClaimHolder()),
            new("ClaimsUnscrubbed", () => new ClaimHolder(), all),
            new("TopClaim", () => new Claim("aud", "x")),
            new(
                "ScrubMembers",
                () => new ScrubTarget(),
                ScrubMembers: ["Secret", "NullSecret", "ZeroSecret", "When"]
            ),
            new(
                "ScrubMembersCollection",
                () => new ScrubCollection(),
                ScrubMembers: ["Secret", "When"]
            ),
            new("GuidProbe", () => new GuidProbe()),
            new("GuidProbeInline", () => new GuidProbe(), GoldenOptions.ScrubInlineGuids),
            new("MultilineScrub", () => new MultilineScrub()),
            new("MultilineScrubUnscrubbed", () => new MultilineScrub(), all),
            new("MultilineScrubInline", () => new MultilineScrub(), GoldenOptions.ScrubInlineGuids),
            new("TopMultilineGuid", () => $"a\n{Guids.A}\nb"),
            new("StringDates", () => new StringDates()),
            new("StringDatesUnscrubbed", () => new StringDates(), GoldenOptions.DontScrubDateTimes),
            new("Offsets", () => new Offsets()),
            new("OffsetsUnscrubbed", () => new Offsets(), GoldenOptions.DontScrubDateTimes),
            new("MemberOrder", () => new MemberOrder()),
            new("Multiline", () => new Multiline()),
            new("Structs", () => new Structs()),
            new("StructsIncluded", () => new Structs(), GoldenOptions.IncludeDefaultValues),
            new("TopGuidString", () => Guids.A.ToString()),
            new("TopInlineGuidString", () => $"x {Guids.A} y", GoldenOptions.ScrubInlineGuids),
            new("TopDateString", () => "2026-02-28T07:46:15Z"),
            new("TopDouble", () => 1.0),
            new("TopDecimal", () => 2.50m),
            new("TopDateTimeOffset", () => new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)),
            new("TopDateOnly", () => new DateOnly(2026, 3, 1)),
            new("TopCrLf", () => "a\r\nb\r\n"),
            new("TopListOfGuids", () => new[] { Guids.A, Guids.A }),
            new("TopListOfDates", () => new[] { new DateOnly(2026, 3, 1) }),
            new("TopObjectArrayMixed", () => new object?[] { 1, "s", null, Color.Red, 1.5 }),
            new("TopBoolTrue", () => true),
            new("TopChar", () => 'c'),
            new("TopEmptyDict", () => new Dictionary<string, int>()),
            new(
                "TopEmptyDictKept",
                () => new Dictionary<string, int>(),
                GoldenOptions.DontIgnoreEmptyCollections
            ),
            new("TopString", () => "hello world"),
            new("TopMultilineString", () => "first\nsecond"),
            new("TopEmptyString", () => ""),
            new("TopInt", () => 5),
            new("TopBool", () => false),
            new("TopNull", () => null),
            new("TopGuid", () => Guids.A),
            new("TopGuidUnscrubbed", () => Guids.A, GoldenOptions.DontScrubGuids),
            new("TopDateTime", () => new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            new("TopEnum", () => Color.Green),
            new(
                "TopEnumList",
                () => new List<Color> { Color.Red, Color.Green, Color.Hovedentreprenør }
            ),
            new("TopStringList", () => new[] { "a", "b,c", "d e" }),
            new("TopEmptyList", () => new List<int>()),
            new(
                "TopEmptyListKept",
                () => new List<int>(),
                GoldenOptions.DontIgnoreEmptyCollections
            ),
            new("TopIntArray", () => new[] { 1, 2, 3 }),
            new("TopEmptyObject", () => new Child()),
            new("TopDictionary", () => new Dictionary<string, int> { ["b"] = 2, ["a"] = 1 }),
            new("Primitives", () => new Primitives()),
            new("Defaults", () => new Defaults()),
            new("DefaultsIncluded", () => new Defaults(), GoldenOptions.IncludeDefaultValues),
            new(
                "DefaultsKeepEmpty",
                () => new Defaults(),
                GoldenOptions.DontIgnoreEmptyCollections
            ),
            new(
                "DefaultsAllOptOuts",
                () => new Defaults(),
                all | GoldenOptions.IncludeDefaultValues | GoldenOptions.DontIgnoreEmptyCollections
            ),
            new("Strings", () => new Strings()),
            new("Times", () => new Times()),
            new("TimesUnscrubbed", () => new Times(), GoldenOptions.DontScrubDateTimes),
            new("Guids", () => new Guids()),
            new("GuidsInline", () => new Guids(), GoldenOptions.ScrubInlineGuids),
            new("GuidsUnscrubbed", () => new Guids(), GoldenOptions.DontScrubGuids),
            new("Nested", () => new Nested()),
            new("NestedKeepEmpty", () => new Nested(), GoldenOptions.DontIgnoreEmptyCollections),
            new("Dictionaries", () => new Dictionaries()),
            new(
                "DictionariesUnscrubbed",
                () => new Dictionaries(),
                all | GoldenOptions.IncludeDefaultValues
            ),
            new(
                "Cycle",
                () =>
                {
                    var parent = new Node { Name = "p" };
                    parent.Children.Add(new Node { Name = "c", Parent = parent });
                    return parent;
                }
            ),
            new(
                "PositionalRecord",
                () => new PositionalRecord("Ola", 30, new Child { Name = "c" })
            ),
            new("DerivedRecord", () => new DerivedRecord()),
            new("WithFields", () => new WithFields()),
            new(
                "Anonymous",
                () =>
                    new
                    {
                        totalPages = 3,
                        totalResults = 30,
                        data = new[]
                        {
                            new
                            {
                                Id = Guids.A,
                                when = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            },
                        },
                    }
            ),
            new(
                "ObjectHolders",
                () =>
                    new List<ObjectHolder>
                    {
                        new() { Value = 1.0 },
                        new() { Value = 0.0 },
                        new() { Value = 3L },
                        new() { Value = Guids.A },
                        new() { Value = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero) },
                        new() { Value = Color.Blue },
                        new() { Value = new[] { 1, 2 } },
                        new() { Value = null },
                        new() { Value = "s" },
                    }
            ),
        ];
    }
}
