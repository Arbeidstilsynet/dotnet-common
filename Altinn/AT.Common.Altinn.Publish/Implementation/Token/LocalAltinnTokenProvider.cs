using System.IdentityModel.Tokens.Jwt;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

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
