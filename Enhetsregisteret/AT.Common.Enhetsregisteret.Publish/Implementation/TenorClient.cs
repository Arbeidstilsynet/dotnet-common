using System.Reflection;
using System.Text;
using System.Text.Json;
using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Response;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using MapsterMapper;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

internal class TenorClient(IMapper mapper, EnhetsregisteretConfig config) : IEnhetsregisteret
{
    private readonly TenorEnhet[] Seed =
        JsonSerializer.Deserialize<TenorEnhet[]>(
            JsonInput(
                config.EmbeddedOrgDataResourceName
                    ?? throw new InvalidOperationException(
                        "Config parameter 'EmbeddedOrgDataResourceName' required when using TenorClient."
                    )
            )
        )
        ?? throw new InvalidOperationException(
            "Could not deserialize test org-data to tenor dto array."
        );

    private static string JsonInput(string embeddedResourceName)
    {
        using var streamReader = new StreamReader(
            Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceName)
                ?? throw new InvalidOperationException(
                    "Required resource stream for in memory test org-data not found."
                )
        );
        return streamReader.ReadToEnd();
    }

    public Task<Enhet?> GetEnhet(string organisasjonsnummer)
    {
        var result = Seed.FirstOrDefault(w =>
            w.Organisasjonsnummer == organisasjonsnummer && w.Underenhet?.Hovedenhet == null
        );

        return Task.FromResult(result != null ? mapper.Map<Enhet>(result) : null);
    }

    public Task<PaginationResult<Oppdatering>?> GetOppdateringerEnheter(
        GetOppdateringerQuery query,
        Pagination pagination
    )
    {
        return Task.FromResult(
            (PaginationResult<Oppdatering>?)
                new PaginationResult<Oppdatering>()
                {
                    TotalElements = 0,
                    PageIndex = pagination.Page,
                    PageSize = pagination.Size,
                }
        );
    }

    public Task<PaginationResult<Oppdatering>?> GetOppdateringerUnderenheter(
        GetOppdateringerQuery query,
        Pagination pagination
    )
    {
        return Task.FromResult(
            (PaginationResult<Oppdatering>?)
                new PaginationResult<Oppdatering>()
                {
                    TotalElements = 0,
                    PageIndex = pagination.Page,
                    PageSize = pagination.Size,
                }
        );
    }

    public Task<Model.Brreg.Underenhet?> GetUnderenhet(string organisasjonsnummer)
    {
        var result = Seed.FirstOrDefault(w =>
            w.Organisasjonsnummer == organisasjonsnummer && w.Underenhet?.Hovedenhet != null
        );

        return Task.FromResult(result != null ? mapper.Map<Model.Brreg.Underenhet>(result) : null);
    }

    public Task<PaginationResult<Enhet>?> SearchEnheter(
        SearchEnheterQuery searchParameters,
        Pagination pagination
    )
    {
        var result = Seed.Where(w =>
            (
                (
                    searchParameters.Organisasjonsnummer.Length == 0
                    || searchParameters.Organisasjonsnummer.Contains(w.Organisasjonsnummer)
                )
                && (
                    searchParameters.Organisasjonsform.Length == 0
                    || searchParameters.Organisasjonsform.Contains(w.Organisasjonsform?.Beskrivelse)
                )
                && (string.IsNullOrEmpty(searchParameters.Navn) || searchParameters.Navn == w.Navn)
                && (
                    string.IsNullOrEmpty(searchParameters.OverordnetEnhetOrganisasjonsnummer)
                    || searchParameters.OverordnetEnhetOrganisasjonsnummer
                        == w.Underenhet.Hovedenhet
                )
            )
            && w.Underenhet?.Hovedenhet == null
        );
        return Task.FromResult(
            (PaginationResult<Enhet>?)
                new PaginationResult<Enhet>()
                {
                    TotalElements = result.Count(),
                    PageIndex = pagination.Page,
                    PageSize = pagination.Size,
                    Elements =
                    [
                        .. result
                            .Take((int)pagination.Size)
                            .Skip((int)pagination.Page * (int)pagination.Size)
                            .Select(s => mapper.Map<Enhet>(result)),
                    ],
                }
        );
    }

    public Task<PaginationResult<Model.Brreg.Underenhet>?> SearchUnderenheter(
        SearchEnheterQuery searchParameters,
        Pagination pagination
    )
    {
        var result = Seed.Where(w =>
            (
                (
                    searchParameters.Organisasjonsnummer.Length == 0
                    || searchParameters.Organisasjonsnummer.Contains(w.Organisasjonsnummer)
                )
                && (
                    searchParameters.Organisasjonsform.Length == 0
                    || searchParameters.Organisasjonsform.Contains(w.Organisasjonsform?.Beskrivelse)
                )
                && (string.IsNullOrEmpty(searchParameters.Navn) || searchParameters.Navn == w.Navn)
                && (
                    string.IsNullOrEmpty(searchParameters.OverordnetEnhetOrganisasjonsnummer)
                    || searchParameters.OverordnetEnhetOrganisasjonsnummer
                        == w.Underenhet.Hovedenhet
                )
            )
            && w.Underenhet?.Hovedenhet != null
        );
        return Task.FromResult(
            (PaginationResult<Model.Brreg.Underenhet>?)
                new PaginationResult<Model.Brreg.Underenhet>()
                {
                    TotalElements = result.Count(),
                    PageIndex = pagination.Page,
                    PageSize = pagination.Size,
                    Elements =
                    [
                        .. result
                            .Take((int)pagination.Size)
                            .Skip((int)pagination.Page * (int)pagination.Size)
                            .Select(s => mapper.Map<Model.Brreg.Underenhet>(result)),
                    ],
                }
        );
    }
}
