using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Authentication;

/// <summary>
/// Attaches an Altinn token to outgoing requests made by the generated clients.
/// </summary>
/// <remarks>
/// This is deliberately not used by the generated authentication client: that client is what
/// exchanges a Maskinporten token for an Altinn token, so authenticating it with an Altinn token
/// would recurse. It uses an <see cref="AnonymousAuthenticationProvider"/> and supplies the
/// Maskinporten bearer token per request instead.
/// </remarks>
internal class AltinnAuthenticationProvider(IAltinnTokenProvider tokenProvider)
    : IAuthenticationProvider
{
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default
    )
    {
        var token = await tokenProvider.GetToken(cancellationToken);

        request.Headers.Add("Authorization", $"Bearer {token}");
    }
}
