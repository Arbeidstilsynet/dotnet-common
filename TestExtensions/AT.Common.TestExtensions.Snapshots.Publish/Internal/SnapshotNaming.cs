using System.Collections;
using System.Globalization;
using System.Text;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Internal;

internal sealed record SnapshotLocation(string Directory, string Name);

internal static class SnapshotNaming
{
    private const string InvalidFileNameCharacters = "<>:\"/\\|?*";

    public static SnapshotLocation Resolve(
        SnapshotSettings settings,
        string sourceFilePath,
        string memberName
    )
    {
        var sourceDirectory = string.IsNullOrEmpty(sourceFilePath)
            ? Environment.CurrentDirectory
            : Path.GetDirectoryName(sourceFilePath) ?? Environment.CurrentDirectory;
        var directory = settings.Directory is null
            ? sourceDirectory
            : Path.GetFullPath(Path.Combine(sourceDirectory, settings.Directory));

        if (settings.FileName is not null)
        {
            return new SnapshotLocation(directory, settings.FileName);
        }

        var test = TestContextReader.Read(memberName);
        var className = test?.TestClass is { } testClass
            ? FormatTypeName(testClass)
            : Path.GetFileNameWithoutExtension(sourceFilePath);
        var methodName = test?.Method?.Name ?? memberName;
        var parameterNames =
            test?.Method?.GetParameters().Select(p => p.Name ?? "").ToArray() ?? [];
        var parameterValues = settings.Parameters ?? test?.Arguments ?? [];

        var name = new StringBuilder();
        name.Append(className).Append('.').Append(methodName);
        var count = Math.Min(parameterNames.Length, parameterValues.Length);
        for (var i = 0; i < count; i++)
        {
            name.Append('_')
                .Append(parameterNames[i])
                .Append('=')
                .Append(Sanitize(FormatParameter(parameterValues[i])));
        }

        return new SnapshotLocation(directory, name.ToString());
    }

    internal static string FormatTypeName(Type type)
    {
        var name = StripArity(type.Name);
        for (var parent = type.DeclaringType; parent is not null; parent = parent.DeclaringType)
        {
            name = StripArity(parent.Name) + "." + name;
        }

        return name;
    }

    internal static string FormatParameter(object? value) =>
        value switch
        {
            null => "null",
            string s => s,
            bool b => b ? "True" : "False",
            Enum e => e.ToString(),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            IEnumerable e => string.Join(",", e.Cast<object?>().Select(FormatParameter)),
            _ => value.ToString() ?? "null",
        };

    internal static string Sanitize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            builder.Append(char.IsControl(c) || InvalidFileNameCharacters.Contains(c) ? '-' : c);
        }

        return builder.ToString();
    }

    private static string StripArity(string name)
    {
        var index = name.IndexOf('`');
        return index < 0 ? name : name[..index];
    }
}
