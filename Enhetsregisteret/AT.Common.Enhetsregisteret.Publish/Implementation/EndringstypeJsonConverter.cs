using System.Text.Json;
using System.Text.Json.Serialization;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

internal class EndringstypeJsonConverter : JsonConverter<Endringstype>
{
    public override Endringstype Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null || reader.GetString() is not { Length: > 0 } value)
        {
            return Endringstype.Ukjent;
        }

        return Enum.TryParse<Endringstype>(value, out var endringstype) ? endringstype : Endringstype.Ukjent;
    }

    public override void Write(Utf8JsonWriter writer, Endringstype value, JsonSerializerOptions options)
    {
        var enumString = value.ToString();
        writer.WriteStringValue(enumString);
    }
}
