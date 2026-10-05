using System.Globalization;
using System.Numerics;
using System.Text;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Serialization;

internal static class ScalarFormatter
{
    public static bool IsScalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(Guid)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan)
            || type == typeof(Uri)
            || type == typeof(Version)
            || type == typeof(byte[])
            || type == typeof(Half)
            || type == typeof(Int128)
            || type == typeof(UInt128)
            || type == typeof(BigInteger)
            || type == typeof(StringBuilder)
            || typeof(Type).IsAssignableFrom(type);
    }

    /// <summary>Formats a scalar nested inside an object or collection, applying scrubbing.</summary>
    public static string Format(object value, ScrubState scrub) =>
        value switch
        {
            string s => scrub.String(s),
            bool b => b ? "true" : "false",
            double d => WithFraction(d.ToString("R", CultureInfo.InvariantCulture)),
            float f => WithFraction(f.ToString("R", CultureInfo.InvariantCulture)),
            decimal m => WithDecimalPoint(m.ToString(CultureInfo.InvariantCulture)),
            Guid g => scrub.Guid(g),
            DateTime dt => scrub.DateTime(dt),
            DateTimeOffset dto => scrub.DateTimeOffset(dto),
            DateOnly d => scrub.DateOnly(d),
            TimeOnly t => scrub.TimeOnly(t),
            _ => FormatPlain(value),
        };

    /// <summary>Formats a scalar written as the whole snapshot. Values are not scrubbed.</summary>
    public static string FormatTopLevel(object value, ScrubState scrub) =>
        value switch
        {
            string s => s.Length == 0 ? "emptyString" : scrub.InlineGuids(s),
            bool b => b ? "True" : "False",
            double d => d.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            _ => FormatPlain(value),
        };

    private static string FormatPlain(object value) =>
        value switch
        {
            string s => s,
            char c => c.ToString(),
            Enum e => e.ToString(),
            Guid g => g.ToString("D"),
            DateTime dt => FormatDateTime(dt),
            DateTimeOffset dto => FormatDateTimeOffset(dto),
            DateOnly d => FormatDateOnly(d),
            TimeOnly t => FormatTimeOnly(t),
            TimeSpan ts => ts.ToString("c", CultureInfo.InvariantCulture),
            Uri uri => uri.OriginalString,
            byte[] bytes => Convert.ToBase64String(bytes),
            Type type => type.FullName ?? type.Name,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "",
        };

    public static string FormatDateTime(DateTime value)
    {
        var text = FormatDateAndTime(value);
        return value.Kind switch
        {
            DateTimeKind.Utc => text + " Utc",
            DateTimeKind.Local => text + " Local",
            _ => text,
        };
    }

    public static string FormatDateTimeOffset(DateTimeOffset value)
    {
        var offset = value.Offset;
        var builder = new StringBuilder(FormatDateAndTime(value.DateTime));
        builder.Append(' ').Append(offset < TimeSpan.Zero ? '-' : '+');
        builder.Append(Math.Abs(offset.Hours).ToString(CultureInfo.InvariantCulture));
        if (offset.Minutes != 0)
        {
            builder.Append('-').Append(Math.Abs(offset.Minutes).ToString("00", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    public static string FormatDateOnly(DateOnly value) =>
        value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string FormatTimeOnly(TimeOnly value) =>
        value.ToString("h:mm tt", CultureInfo.InvariantCulture);

    private static string FormatDateAndTime(DateTime value)
    {
        var text = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (value.TimeOfDay == TimeSpan.Zero)
        {
            return text;
        }

        text += value.ToString(" HH:mm:ss", CultureInfo.InvariantCulture);
        var fraction = value.Ticks % TimeSpan.TicksPerSecond;
        if (fraction != 0)
        {
            text += "." + fraction.ToString("0000000", CultureInfo.InvariantCulture).TrimEnd('0');
        }

        return text;
    }

    private static string WithFraction(string number) =>
        number.AsSpan().IndexOfAny(".EeNI") >= 0 ? number : number + ".0";

    private static string WithDecimalPoint(string number) =>
        number.Contains('.') ? number : number + ".0";
}
