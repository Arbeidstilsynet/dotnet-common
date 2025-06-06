using Arbeidstilsynet.Common.EraClient.Model;

namespace Arbeidstilsynet.Common.EraClient;

/// <summary>
/// Interface which can be dependency injected to use methods of EraClient
/// </summary>
public interface IAuthenticationClient
{
    Task<AuthenticationResponseDto> Authenticate(AuthenticationRequestDto authenticationRequestDto);
}
