using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Authentication;

/// <summary>
/// Attaches an Altinn token to outgoing requests made by the generated clients.
/// </summary>
/// <remarks>
/// <para>
/// Each instance carries the scopes of the client it authenticates, so a client only ever
/// presents a token holding the scopes it was registered with, rather than the union of everything
/// the application is entitled to.
/// </para>
/// <para>
/// This is deliberately not used by the generated authentication client: that client is what
/// exchanges a Maskinporten token for an Altinn token, so authenticating it with an Altinn token
/// would recurse. It uses an <see cref="AnonymousAuthenticationProvider"/> and supplies the
/// Maskinporten bearer token per request instead.
/// </para>
/// </remarks>
internal class AltinnAuthenticationProvider(
    IAltinnTokenProvider tokenProvider,
    IReadOnlyList<string> scopes
) : IAuthenticationProvider
{
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default
    )
    {
        var token = await tokenProvider.GetToken(scopes, cancellationToken);

        request.Headers.Add("Authorization", $"Bearer {token}");
    }
}
