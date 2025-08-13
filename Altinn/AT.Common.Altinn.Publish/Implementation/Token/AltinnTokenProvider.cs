using System.IdentityModel.Tokens.Jwt;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Ports;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnTokenProvider(
    IAltinnTokenAdapter altinnTokenAdapter,
    IOptions<AltinnAuthenticationConfiguration> altinnAuthenticationConfiguration
) : IAltinnTokenProvider
{
    public async Task<string> GetToken()
    {
        var config = altinnAuthenticationConfiguration.Value;
        var jwtToken = await altinnTokenAdapter.StartTokenExchange(
            config.CertificatePrivateKey,
            config.IntegrationId,
            config.Scopes
        );
        return jwtToken;
    }
}
