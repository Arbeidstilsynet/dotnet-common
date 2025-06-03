using Arbeidstilsynet.Common.EraClient.Ports.Model;

namespace Arbeidstilsynet.Common.EraClient.Ports;

/// <summary>
/// Interface which can be dependency injected to use methods of EraClient
/// </summary>
public interface IAuthenticationClient
{
    Task<AuthenticationResponseDto> Authenticate(AuthenticationRequestDto authenticationRequestDto);
}
