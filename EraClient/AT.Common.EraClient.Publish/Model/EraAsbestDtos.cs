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

public record GodkjenningStatusResponse
{
    public string Organisasjonsnummer { get; init; }
    public Registerstatus Registerstatus { get; init; }
    public Godkjenningstype Godkjenningstype { get; init; }
    public string GodkjenningstypeBeskrivelse { get; init; }
    public string TillatelseUtloper { get; init; }
}

public record BehandlingsstatusResponse
{
    public string Organisasjonsnummer { get; set; }
    public bool KanSendeSoknad { get; set; }
    public string Aarsak { get; set; }
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
