using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Validation.Extensions;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

internal static class QueryExtensions
{
    public static IReadOnlyDictionary<string, string> ToMap(this SearchEnheterQuery query)
    {
        var parameterMap = new Dictionary<string, string>();

        if (query is { Navn.Length: > 0 })
        {
            parameterMap.Add("navn", query.Navn);
        }

        var validOrgnumre = query.Organisasjonsnummer.Where(o => o.IsValidOrgnummer()).ToArray();

        if (validOrgnumre.Length != 0)
        {
            parameterMap.Add("organisasjonsnummer", string.Join(",", validOrgnumre));
        }

        if (query.Organisasjonsform.Length != 0)
        {
            parameterMap.Add("organisasjonsform", string.Join(",", query.Organisasjonsform));
        }

        if (query is { OverordnetEnhetOrganisasjonsnummer.Length: 9 })
        {
            parameterMap.Add("overordnetEnhet", query.OverordnetEnhetOrganisasjonsnummer);
        }

        if (query is { StrictSearch: true })
        {
            parameterMap.Add("navnMetodeForSoek", "FORTLOEPENDE");
        }

        var sortDirection = query.SortDirection.ToString().ToLower();

        if (query is { SortBy.Length: > 0 })
        {
            parameterMap.Add("sort", $"{query.SortBy},{sortDirection}");
        }
        else
        {
            parameterMap.Add("sort", sortDirection);
        }

        return parameterMap;
    }

    public static IReadOnlyDictionary<string, string> ToMap(this Pagination pagination)
    {
        var parameterMap = new Dictionary<string, string>();

        var page = pagination.Page >= 0 ? pagination.Page.ToString() : "0";

        parameterMap.Add("page", page);

        var size = pagination.Size > 0 ? pagination.Size.ToString() : "1000";

        parameterMap.Add("size", size);

        return parameterMap;
    }

    public static IReadOnlyDictionary<string, string> ToMap(this GetOppdateringerQuery query)
    {
        var parameterMap = new Dictionary<string, string>();

        var validOrgnumre = query.Organisasjonsnummer.Where(o => o.IsValidOrgnummer()).ToArray();

        if (validOrgnumre.Length != 0)
        {
            parameterMap.Add("organisasjonsnummer", string.Join(",", validOrgnumre));
        }

        parameterMap.Add("oppdateringsid", query.Oppdateringsid.ToString());
        parameterMap.Add("dato", query.Dato.ToUniversalTime().ToString("o"));

        return parameterMap;
    }
}
