using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Model.Api;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for Maskinporten Authentication
/// </summary>
public interface IMaskinportenClient
{
    Task<MaskinportenTokenResponse> GetToken(string jwtGrant);

    Uri BaseUrl { get; }
}
