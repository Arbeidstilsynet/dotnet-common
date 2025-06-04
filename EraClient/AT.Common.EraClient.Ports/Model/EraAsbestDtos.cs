using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.EraClient.Ports.Model.Asbest;

public record Melding
{
    [property: JsonPropertyName("Arkivreferanse")]
    Arkivreferanse Arkivreferanse { get; init; }

    [property: JsonPropertyName("StartDato")]
    DateTime StartDato { get; init; }

    [property: JsonPropertyName("SluttDato")]
    DateTime SluttDato { get; init; }

    [property: JsonPropertyName("OppdragGateadresse")]
    string OppdragGateadresse { get; init; }

    [property: JsonPropertyName("OppdragPostnummer")]
    string OppdragPostnummer { get; init; }

    [property: JsonPropertyName("OppdragPoststed")]
    string OppdragPoststed { get; init; }
};

public record Arkivreferanse
{
    [property: JsonPropertyName("SaksId")]
    string SaksId { get; init; }

    [property: JsonPropertyName("Saksnummer")]
    string Saksnummer { get; init; }

    [property: JsonPropertyName("JournalpostId")]
    string JournalpostId { get; init; }
};
