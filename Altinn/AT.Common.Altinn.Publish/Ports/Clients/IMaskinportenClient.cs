using Arbeidstilsynet.Common.Altinn.Implementation.Clients;
using Arbeidstilsynet.Common.Altinn.Model.Api;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for Maskinporten Authentication
/// </summary>
public interface IMaskinportenClient
{
    /// <summary>
    /// Get a Maskinporten token
    /// </summary>
    /// <returns></returns>
    Task<MaskinportenTokenResponse> GetToken();
}
