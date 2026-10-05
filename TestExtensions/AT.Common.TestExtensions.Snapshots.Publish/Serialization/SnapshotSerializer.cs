using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Arbeidstilsynet.Common.TestExtensions.Snapshots.Serialization;

/// <summary>Reflection-based writer for the snapshot text format described in SPEC.md §3.</summary>
internal sealed class SnapshotSerializer
{
    private const int IndentSize = 2;

    private static readonly ConcurrentDictionary<Type, MemberAccessor[]> MemberCache = new();
    private static readonly ConcurrentDictionary<Type, object?> DefaultCache = new();

    private readonly SnapshotSettings _settings;
    private readonly ScrubState _scrub;
    private readonly StringBuilder _output = new();
    private const int MaxDepth = 64;

    private readonly HashSet<object> _ancestors = new(ReferenceEqualityComparer.Instance);
    private int _depth;

    private SnapshotSerializer(SnapshotSettings settings)
    {
        _settings = settings;
        _scrub = new ScrubState(settings);
    }

    public static string Serialize(object? value, SnapshotSettings settings)
    {
        var serializer = new SnapshotSerializer(settings);
        var text = serializer.SerializeRoot(value);
        foreach (var scrubber in settings.Scrubbers)
        {
            text = scrubber(text);
        }

        return text;
    }

    private string SerializeRoot(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        if (ScalarFormatter.IsScalar(value.GetType()))
        {
            return NormalizeNewLines(ScalarFormatter.FormatTopLevel(value, _scrub));
        }

        var prepared = Prepare(value);
        switch (prepared)
        {
            case ListValue { Items.Count: 0 }:
                return "[]";
            case DictValue { Entries.Count: 0 }:
                return "{}";
        }

        WriteComplex(prepared, 0);
        return _output.ToString();
    }

    // ---------- preparation ----------

    private sealed record ListValue(object Source, List<object?> Items);

    private sealed record DictValue(object Source, List<KeyValuePair<object, object?>> Entries);

    private sealed record JsonValueWrapper(JsonElement Element);

    private static object Prepare(object value)
    {
        switch (value)
        {
            case JsonElement element:
                return new JsonValueWrapper(element);
            case JsonDocument document:
                return new JsonValueWrapper(document.RootElement);
            case JsonNode node:
                return new JsonValueWrapper(JsonSerializer.SerializeToElement(node));
        }

        var type = value.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            var (key, item) = ReadPair(value);
            return new DictValue(value, [new(key ?? "", item)]);
        }

        if (value is IDictionary dictionary)
        {
            var entries = new List<KeyValuePair<object, object?>>();
            foreach (DictionaryEntry entry in dictionary)
            {
                entries.Add(new(entry.Key, entry.Value));
            }

            return new DictValue(value, SortEntries(entries));
        }

        if (value is IEnumerable enumerable)
        {
            if (IsGenericDictionary(type))
            {
                var entries = new List<KeyValuePair<object, object?>>();
                foreach (var pair in enumerable)
                {
                    var (key, item) = ReadPair(pair!);
                    entries.Add(new(key ?? "", item));
                }

                return new DictValue(value, SortEntries(entries));
            }

            var items = new List<object?>();
            foreach (var item in enumerable)
            {
                items.Add(item);
            }

            return new ListValue(value, items);
        }

        return value;
    }

    private static bool IsGenericDictionary(Type type) =>
        type.GetInterfaces()
            .Append(type)
            .Any(i =>
                i.IsGenericType
                && (
                    i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                )
            );

    private static (object? Key, object? Value) ReadPair(object pair)
    {
        var type = pair.GetType();
        return (type.GetProperty("Key")?.GetValue(pair), type.GetProperty("Value")?.GetValue(pair));
    }

    private static List<KeyValuePair<object, object?>> SortEntries(
        List<KeyValuePair<object, object?>> entries
    )
    {
        if (entries.All(e => e.Key is string))
        {
            return
            [
                .. entries
                    .OrderBy(e => (string)e.Key, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(e => (string)e.Key, StringComparer.Ordinal),
            ];
        }

        try
        {
            return [.. entries.OrderBy(e => e.Key, Comparer<object>.Default)];
        }
        catch (InvalidOperationException)
        {
            return [.. entries.OrderBy(e => e.Key.ToString(), StringComparer.OrdinalIgnoreCase)];
        }
    }

    private static bool IsEmpty(object prepared) =>
        prepared switch
        {
            ListValue list => list.Items.Count == 0,
            DictValue dict => dict.Entries.Count == 0,
            byte[] bytes => bytes.Length == 0,
            _ => false,
        };

    private bool IsCycle(object value) =>
        !value.GetType().IsValueType && _ancestors.Contains(value);

    // ---------- writing ----------

    private void WriteComplex(object prepared, int indent)
    {
        var source = prepared switch
        {
            ListValue list => list.Source,
            DictValue dict => dict.Source,
            _ => prepared,
        };
        if (_depth >= MaxDepth)
        {
            throw new InvalidOperationException(
                $"Snapshot serialization exceeded the maximum depth of {MaxDepth} at '{source.GetType().FullName}'. "
                    + "The object graph is probably unbounded (e.g. a getter that returns a new object on every call). "
                    + "Snapshot a projection of the value instead."
            );
        }

        var tracked = !source.GetType().IsValueType && _ancestors.Add(source);
        _depth++;
        try
        {
            switch (prepared)
            {
                case ListValue list:
                    WriteList(list, indent);
                    break;
                case DictValue dict:
                    WriteDictionary(dict, indent);
                    break;
                case JsonValueWrapper json:
                    WriteJson(json.Element, indent);
                    break;
                default:
                    WriteObject(prepared, indent);
                    break;
            }
        }
        finally
        {
            _depth--;
            if (tracked)
            {
                _ancestors.Remove(source);
            }
        }
    }

    private void WriteList(ListValue list, int indent)
    {
        var lines = new List<Action>();
        foreach (var item in list.Items)
        {
            if (item is null)
            {
                lines.Add(() => _output.Append("null"));
                continue;
            }

            if (ScalarFormatter.IsScalar(item.GetType()))
            {
                if (item is byte[] { Length: 0 } && _settings.IgnoreEmptyCollectionsEnabled)
                {
                    continue;
                }

                lines.Add(() =>
                    _output.Append(NormalizeNewLines(ScalarFormatter.Format(item, _scrub)))
                );
                continue;
            }

            if (IsCycle(item))
            {
                continue;
            }

            var prepared = Prepare(item);
            if (IsEmpty(prepared) && _settings.IgnoreEmptyCollectionsEnabled)
            {
                continue;
            }

            lines.Add(() => WriteComplex(prepared, indent + IndentSize));
        }

        WriteBlock('[', ']', lines, indent);
    }

    private void WriteDictionary(DictValue dict, int indent)
    {
        var entries = new List<(Func<string> Label, object? Value)>();
        foreach (var (key, value) in dict.Entries)
        {
            if (ShouldSkipEntryValue(value, out var prepared))
            {
                continue;
            }

            entries.Add((() => FormatKey(key), prepared));
        }

        WriteEntries(entries, indent);
    }

    private void WriteObject(object value, int indent)
    {
        var entries = new List<(Func<string> Label, object? Value)>();
        foreach (var member in GetMembers(value.GetType()))
        {
            var memberValue = member.GetValue(value);
            if (ShouldSkipMember(member, memberValue, out var prepared))
            {
                continue;
            }

            var name = member.Name;
            entries.Add((() => name, prepared));
        }

        WriteEntries(entries, indent);
    }

    private bool ShouldSkipMember(MemberAccessor member, object? value, out object? prepared)
    {
        prepared = null;
        if (value is null)
        {
            return !_settings.IncludeDefaultValuesEnabled;
        }

        if (value is Delegate || IsCycle(value))
        {
            return true;
        }

        if (
            !_settings.IncludeDefaultValuesEnabled
            && member.Type.IsValueType
            && member.Type != typeof(bool)
            && Equals(value, GetDefault(member.Type))
        )
        {
            return true;
        }

        return ShouldSkipEntryValue(value, out prepared);
    }

    /// <summary>Shared rule for object members and dictionary values: only cycles and empty collections are skipped.</summary>
    private bool ShouldSkipEntryValue(object? value, out object? prepared)
    {
        prepared = null;
        if (value is null)
        {
            return false;
        }

        if (value is Delegate || IsCycle(value))
        {
            return true;
        }

        prepared = ScalarFormatter.IsScalar(value.GetType()) ? value : Prepare(value);
        if (IsEmpty(prepared) && _settings.IgnoreEmptyCollectionsEnabled)
        {
            if (!_settings.IncludeDefaultValuesEnabled)
            {
                return true;
            }

            prepared = null;
        }

        return false;
    }

    private void WriteEntries(List<(Func<string> Label, object? Value)> entries, int indent)
    {
        var lines = new List<Action>(entries.Count);
        foreach (var (label, value) in entries)
        {
            lines.Add(() =>
            {
                _output.Append(label()).Append(':');
                WriteEntryValue(value, indent + IndentSize);
            });
        }

        WriteBlock('{', '}', lines, indent);
    }

    private void WriteEntryValue(object? prepared, int indent)
    {
        if (prepared is null)
        {
            _output.Append(" null");
            return;
        }

        if (ScalarFormatter.IsScalar(prepared.GetType()))
        {
            var text = NormalizeNewLines(ScalarFormatter.Format(prepared, _scrub));
            _output.Append(text.Contains('\n') ? "\n" : " ").Append(text);
            return;
        }

        _output.Append(' ');
        WriteComplex(prepared, indent);
    }

    private void WriteBlock(char open, char close, List<Action> lines, int indent)
    {
        if (lines.Count == 0)
        {
            _output.Append(open).Append(close);
            return;
        }

        _output.Append(open);
        for (var i = 0; i < lines.Count; i++)
        {
            _output.Append('\n').Append(' ', indent + IndentSize);
            lines[i]();
            if (i < lines.Count - 1)
            {
                _output.Append(',');
            }
        }

        _output.Append('\n').Append(' ', indent).Append(close);
    }

    private string FormatKey(object key) =>
        key switch
        {
            _ when ScalarFormatter.IsScalar(key.GetType()) => NormalizeNewLines(
                ScalarFormatter.Format(key, _scrub)
            ),
            _ => key.ToString() ?? "",
        };

    private void WriteJson(JsonElement element, int indent)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var properties = element
                    .EnumerateObject()
                    .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(p => p.Name, StringComparer.Ordinal)
                    .ToList();
                var lines = new List<Action>(properties.Count);
                foreach (var property in properties)
                {
                    lines.Add(() =>
                    {
                        _output.Append(NormalizeNewLines(_scrub.String(property.Name))).Append(':');
                        WriteJsonEntryValue(property.Value, indent + IndentSize);
                    });
                }

                WriteBlock('{', '}', lines, indent);
                break;
            }
            case JsonValueKind.Array:
            {
                var lines = new List<Action>();
                foreach (var item in element.EnumerateArray())
                {
                    lines.Add(() => WriteJsonItem(item, indent + IndentSize));
                }

                WriteBlock('[', ']', lines, indent);
                break;
            }
            default:
                WriteJsonItem(element, indent);
                break;
        }
    }

    private void WriteJsonEntryValue(JsonElement element, int indent)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var text = NormalizeNewLines(_scrub.String(element.GetString()!));
            _output.Append(text.Contains('\n') ? "\n" : " ").Append(text);
            return;
        }

        _output.Append(' ');
        WriteJson(element, indent);
    }

    private void WriteJsonItem(JsonElement element, int indent)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object or JsonValueKind.Array:
                WriteJson(element, indent);
                break;
            case JsonValueKind.String:
                _output.Append(NormalizeNewLines(_scrub.String(element.GetString()!)));
                break;
            case JsonValueKind.True:
                _output.Append("true");
                break;
            case JsonValueKind.False:
                _output.Append("false");
                break;
            case JsonValueKind.Number:
                _output.Append(element.GetRawText());
                break;
            default:
                _output.Append("null");
                break;
        }
    }

    private static string NormalizeNewLines(string text) =>
        text.Contains('\r') ? text.Replace("\r\n", "\n").Replace('\r', '\n') : text;

    // ---------- reflection ----------

    private static object? GetDefault(Type type) =>
        DefaultCache.GetOrAdd(type, Activator.CreateInstance);

    private static MemberAccessor[] GetMembers(Type type) =>
        MemberCache.GetOrAdd(type, BuildMembers);

    private const BindingFlags MemberFlags =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static MemberAccessor[] BuildMembers(Type type)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var members = new List<MemberAccessor>();
        var hierarchy = TypeHierarchy(type).ToList();

        foreach (var field in hierarchy.SelectMany(t => t.GetFields(MemberFlags)))
        {
            if (!field.FieldType.IsByRefLike && seen.Add(field.Name))
            {
                members.Add(new MemberAccessor(field.Name, field.FieldType, type, field.GetValue));
            }
        }

        foreach (var property in hierarchy.SelectMany(t => t.GetProperties(MemberFlags)))
        {
            if (IsSerializable(property) && seen.Add(property.Name))
            {
                members.Add(
                    new MemberAccessor(
                        property.Name,
                        property.PropertyType,
                        type,
                        property.GetValue
                    )
                );
            }
        }

        return [.. members];
    }

    private static IEnumerable<Type> TypeHierarchy(Type type)
    {
        for (
            var current = type;
            current is not null && current != typeof(object);
            current = current.BaseType
        )
        {
            yield return current;
        }
    }

    private static bool IsSerializable(PropertyInfo property) =>
        property.GetMethod is { IsPublic: true }
        && property.GetIndexParameters().Length == 0
        && !property.PropertyType.IsByRefLike
        && !property.PropertyType.IsPointer;

    private sealed class MemberAccessor(
        string name,
        Type type,
        Type owner,
        Func<object?, object?> getter
    )
    {
        public string Name { get; } = name;
        public Type Type { get; } = type;

        public object? GetValue(object instance)
        {
            try
            {
                return getter(instance);
            }
            catch (TargetInvocationException exception)
            {
                throw new InvalidOperationException(
                    $"Error getting value from '{Name}' on '{owner}'.",
                    exception.InnerException ?? exception
                );
            }
        }
    }
}
