namespace Arbeidstilsynet.Common.Saksarkiv.Ports;

/// <summary>
/// Provides access tokens used when authenticating requests to the Saksarkiv API.
/// </summary>
public interface ISaksarkivTokenProvider
{
    /// <summary>
    /// Gets an access token for the provided OAuth scope.
    /// </summary>
    /// <param name="scope">The OAuth scope to request a token for.</param>
    /// <param name="cancellationToken">A token used to cancel the token request.</param>
    /// <returns>The access token value.</returns>
    Task<string> GetAccessToken(string scope, CancellationToken cancellationToken = default);
}
