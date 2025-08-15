using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions;

public class JsonStringUriConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(Uri);
    }

    public override JsonConverter? CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (typeToConvert != typeof(Uri))
        {
            throw new ArgumentOutOfRangeException();
        }
        return new UriConverter();
    }
}
