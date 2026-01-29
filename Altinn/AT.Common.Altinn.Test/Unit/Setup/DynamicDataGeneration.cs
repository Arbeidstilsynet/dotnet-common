using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
using WireMock.Net.OpenApiParser.Settings;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.Setup;

public class DynamicDataGeneration : WireMockOpenApiParserDynamicExampleValues
{
    internal static Guid DefaultPathUuid = Guid.NewGuid();

    internal static int DefaultIntValue = 42;

    public override int Integer => DefaultIntValue;
    public override string String
    {
        get
        {
            // Since you have your Schema, you can get if max-length is set. You can generate accurate examples with this settings
            var maxLength = Schema?.MaxLength ?? 9;

            var isUuid = Schema?.Format == "uuid";

            if (isUuid)
            {
                return DefaultPathUuid.ToString();
            }

            return RandomizerFactory
                    .GetRandomizer(
                        new FieldOptionsTextRegex { Pattern = $"[0-9A-Z]{{{maxLength}}}" }
                    )
                    .Generate() ?? "example-string";
        }
    }
}
