using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

public class UriConverter : JsonConverter<Uri>
{
    public override Uri? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (typeToConvert == typeof(Uri))
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return new Uri(value);
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
