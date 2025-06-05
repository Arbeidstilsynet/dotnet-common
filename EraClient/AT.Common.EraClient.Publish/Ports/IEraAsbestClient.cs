using Arbeidstilsynet.Common.EraClient.Model;
using Arbeidstilsynet.Common.EraClient.Model.Asbest;

namespace Arbeidstilsynet.Common.EraClient;

/// <summary>
/// Interface which can be dependency injected to use asbest endpoints of the EraClient
/// </summary>
public interface IEraAsbestClient
{
    Task<List<Model.Asbest.Melding>> GetMeldingerByOrg(
        AuthenticationResponseDto authenticationResponse,
        string orgNumber
    );

    Task<SøknadStatusResponse?> GetStatusForExistingSøknad(
        AuthenticationResponseDto authenticationResponse,
        string orgNumber
    );
}
