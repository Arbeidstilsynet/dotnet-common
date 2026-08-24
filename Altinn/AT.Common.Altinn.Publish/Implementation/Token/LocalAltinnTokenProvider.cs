using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Token;

/// <summary>
/// Obtains a test token from a locally running Altinn instance.
/// </summary>
/// <remarks>
/// Only registered when the resolved target is <see cref="AltinnEnvironment.Local"/>: the tokens
/// this endpoint issues are accepted by a local Altinn instance only.
/// </remarks>
internal class LocalAltinnTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<AltinnConfiguration> configuration,
    ResolvedAltinnUrls urls
) : IAltinnTokenProvider
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    public async Task<string> GetToken(CancellationToken cancellationToken = default)
    {
        var tokenUrl = new Uri(
            urls.AppBaseUrl,
            $"Home/GetTestOrgToken?org={configuration.Value.OrgId}&authenticationLevel=2&orgNumber=&scopes="
        );

        return await _httpClient.GetStringAsync(tokenUrl, cancellationToken);
    }
}
