using System.Text.Json.Serialization;

namespace Arbeidstilsynet.Common.EraClient.Model.Asbest;

public record Melding
{
    public required Arkivreferanse Arkivreferanse { get; init; }

    public required DateTime StartDato { get; init; }

    public required DateTime SluttDato { get; init; }

    public required string OppdragGateadresse { get; init; }

    public required string OppdragPostnummer { get; init; }

    public required string OppdragPoststed { get; init; }
};

public record Arkivreferanse
{
    public string? SaksId { get; init; }

    public string? Saksnummer { get; init; }

    public string? JournalpostId { get; init; }
};

public record SøknadStatusResponse
{
    public required string SøknadId { get; init; }
    public required Søknadstatus Sakstatus { get; init; }
    public required string ArkivSakId { get; init; }
    public required string ArkivSaknummer { get; init; }
    public required List<Mangelkategori> Mangelkategorier { get; init; }
}

public record Mangelkategori
{
    public required string Navn { get; init; }
    public required List<string> Mangelbeskrivelser { get; init; }
}

public record GodkjenningStatusResponse
{
    public required string Organisasjonsnummer { get; init; }
    public required string Registerstatus { get; init; }
    public Registerstatus? RegisterstatusEnum { get; init; }
    public required string Godkjenningstype { get; init; }
    public Godkjenningstype? GodkjenningstypeEnum { get; init; }
    public required string TillatelseUtloper { get; init; }
}

public record BehandlingsstatusResponse
{
    public required string Organisasjonsnummer { get; set; }
    public required bool KanSendeSoknad { get; set; }
    public required string Aarsak { get; set; }
}

public enum Godkjenningstype
{
    Ingen,
    HåndverkArbeid,
    Rør,
    Utvendig,
    Innvendig,
}

public enum Registerstatus
{
    IkkeRegistrert,
    Registrert,
}

public enum Søknadstatus
{
    Ukjent,
    Underbehandling,
    Avsluttet,
}
