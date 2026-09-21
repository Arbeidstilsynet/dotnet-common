using Arbeidstilsynet.Common.Saksarkiv.V3;

namespace Arbeidstilsynet.Common.Saksarkiv.Implementation;

/// <summary>
/// Thin seam over the generated <see cref="SaksarkivClientV3"/> health probe, introduced so the
/// health check can be unit tested without the Kiota-generated client.
/// </summary>
/// <remarks>
/// Probes the version-agnostic <c>GET /api/health/authPing</c> endpoint, which verifies
/// authentication and basic access and is shared across all API versions.
/// </remarks>
internal interface ISaksarkivHealthPinger
{
    Task<string?> PongAsync(CancellationToken cancellationToken = default);
}

internal class SaksarkivHealthPinger(SaksarkivClientV3 saksarkivClient) : ISaksarkivHealthPinger
{
    public async Task<string?> PongAsync(CancellationToken cancellationToken = default)
    {
        var response = await saksarkivClient.Api.Health.AuthPing.GetAsync(
            cancellationToken: cancellationToken
        );

        return $"authPing: {response ?? "<null>"}";
    }
}
