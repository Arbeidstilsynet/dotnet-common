using Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Arbeidstilsynet.Shared.Implementation;
using PunktQueryParameters = Arbeidstilsynet.Common.GeoNorge.KommuneInfo.Punkt.PunktRequestBuilder.PunktRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal sealed class FylkeKommuneStub : IFylkeKommuneApi
{
    public Task<IEnumerable<FylkerEnkel>> GetFylker() =>
        Task.FromResult<IEnumerable<FylkerEnkel>>([
            new() { Fylkesnummer = "03", Fylkesnavn = "Oslo" },
        ]);

    public Task<IEnumerable<KomEnkelNorskNavn>> GetKommuner() =>
        Task.FromResult<IEnumerable<KomEnkelNorskNavn>>([
            new()
            {
                Kommunenummer = "0301",
                Kommunenavn = "Oslo",
                KommunenavnNorsk = "Oslo",
            },
        ]);

    public Task<IEnumerable<FylkerKommunerFull>> GetFylkerFullInfo() =>
        Task.FromResult<IEnumerable<FylkerKommunerFull>>([
            new()
            {
                Fylkesnummer = "03",
                Fylkesnavn = "Oslo",
                Kommuner = [NewKommune()],
            },
        ]);

    public Task<FylkerKommunerEnkel?> GetFylkeByNumber(string fylkesnummer) =>
        Task.FromResult<FylkerKommunerEnkel?>(
            fylkesnummer == "03"
                ? new()
                {
                    Fylkesnummer = "03",
                    Fylkesnavn = "Oslo",
                    Kommuner = [new() { Kommunenummer = "0301", Kommunenavn = "Oslo" }],
                }
                : null
        );

    public Task<KomFull?> GetKommuneByNumber(string kommunenummer) =>
        Task.FromResult(kommunenummer == "0301" ? NewKommune() : null);

    public Task<KommuneFylkeEnkel?> GetKommuneByPoint(PunktQueryParameters queryParameters)
    {
        StubQuery.Validate(queryParameters, "Nord", "Ost", "Koordsys");
        StubQuery.GeographicCoordinates(queryParameters.Koordsys);
        return Task.FromResult<KommuneFylkeEnkel?>(
            new()
            {
                Fylkesnummer = "03",
                Fylkesnavn = "Oslo",
                Kommunenummer = "0301",
                Kommunenavn = "Oslo",
            }
        );
    }

    private static KomFull NewKommune() =>
        new()
        {
            Fylkesnummer = "03",
            Fylkesnavn = "Oslo",
            Kommunenummer = "0301",
            Kommunenavn = "Oslo",
            KommunenavnNorsk = "Oslo",
        };
}
