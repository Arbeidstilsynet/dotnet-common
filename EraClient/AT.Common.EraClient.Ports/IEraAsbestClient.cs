using Arbeidstilsynet.Common.EraClient.Ports.Model;

namespace Arbeidstilsynet.Common.EraClient.Ports;

/// <summary>
/// Interface which can be dependency injected to use asbest endpoints of the EraClient
/// </summary>
public interface IEraAsbestClient
{
    Task<List<Model.Asbest.Melding>> GetMeldingerByOrg(
        AuthenticationResponseDto authenticationResponse,
        string orgNumber
    );
}
