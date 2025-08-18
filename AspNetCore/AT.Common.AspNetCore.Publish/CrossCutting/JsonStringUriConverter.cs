using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <inheritdoc />
public sealed class JsonStringUriConverter : JsonConverter<Uri>
{
    /// <inheritdoc />
    public override Uri? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Unexpected token parsing Uri. Expected String, got {reader.TokenType}."
            );
        }

        var uriString = reader.GetString();
        if (string.IsNullOrWhiteSpace(uriString))
        {
            return null; // matches behavior for empty/whitespace
        }

        if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
        {
            return uri;
        }

        throw new JsonException($"Invalid URI: '{uriString}'.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Uri? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
