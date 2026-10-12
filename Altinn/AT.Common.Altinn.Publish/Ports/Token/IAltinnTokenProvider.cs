namespace Arbeidstilsynet.Common.Altinn.Ports.Token;

/// <summary>
/// Provides access to Altinn authentication tokens.
/// </summary>
public interface IAltinnTokenProvider
{
    /// <summary>
    /// Gets an authentication token for Altinn API requests.
    /// </summary>
    /// <param name="scopes">
    /// The Maskinporten scopes the token should carry. Tokens are obtained per distinct scope set,
    /// so a client only ever presents the scopes it was registered with.
    /// </param>
    /// <param name="cancellationToken">Cancels the token request.</param>
    /// <returns>A valid Altinn API token as a string.</returns>
    Task<string> GetToken(
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default
    );
}
