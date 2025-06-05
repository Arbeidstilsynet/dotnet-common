using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.EraClient.Model.Asbest;

public record Melding
{
    public Arkivreferanse Arkivreferanse { get; init; }

    public DateTime StartDato { get; init; }

    public DateTime SluttDato { get; init; }

    public string OppdragGateadresse { get; init; }

    public string OppdragPostnummer { get; init; }

    public string OppdragPoststed { get; init; }
};

public record Arkivreferanse
{
    public string SaksId { get; init; }

    public string Saksnummer { get; init; }

    public string JournalpostId { get; init; }
};

public record SøknadStatusResponse
{
    public string SøknadId { get; init; }
    public Søknadstatus Sakstatus { get; init; }
    public string ArkivSakId { get; init; }
    public string ArkivSaknummer { get; init; }
    public List<Mangelkategori> Mangelkategorier { get; init; }
}

public record Mangelkategori
{
    public string Navn { get; init; }
    public List<string> Mangelbeskrivelser { get; init; }
}

public enum Søknadstatus
{
    Ukjent,
    Underbehandling,
    Avsluttet,
}
