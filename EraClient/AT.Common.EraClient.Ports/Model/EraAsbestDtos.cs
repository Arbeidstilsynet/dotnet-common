using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.EraClient.Ports.Model.Asbest;

public record Melding
{
    [property: JsonPropertyName("Arkivreferanse")]
    public Arkivreferanse Arkivreferanse { get; init; }

    [property: JsonPropertyName("StartDato")]
    public DateTime StartDato { get; init; }

    [property: JsonPropertyName("SluttDato")]
    public DateTime SluttDato { get; init; }

    [property: JsonPropertyName("OppdragGateadresse")]
    public string OppdragGateadresse { get; init; }

    [property: JsonPropertyName("OppdragPostnummer")]
    public string OppdragPostnummer { get; init; }

    [property: JsonPropertyName("OppdragPoststed")]
    public string OppdragPoststed { get; init; }
};

public record Arkivreferanse
{
    [property: JsonPropertyName("SaksId")]
    public string SaksId { get; init; }

    [property: JsonPropertyName("Saksnummer")]
    public string Saksnummer { get; init; }

    [property: JsonPropertyName("JournalpostId")]
    public string JournalpostId { get; init; }
};
