using Arbeidstilsynet.Common.Altinn.Ports.Token;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Token;

internal class LocalAltinnTokenProvider : IAltinnTokenProvider
{
    private readonly HttpClient _httpClient;

    public LocalAltinnTokenProvider()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> GetToken()
    {
        return await _httpClient.GetStringAsync(
            "http://local.altinn.cloud/Home/GetTestOrgToken?org=dat&authenticationLevel=2&orgNumber=&scopes="
        );
    }
}
