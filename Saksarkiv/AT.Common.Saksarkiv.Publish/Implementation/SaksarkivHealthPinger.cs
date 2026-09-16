using Arbeidstilsynet.Common.Saksarkiv.V3;

namespace Arbeidstilsynet.Common.Saksarkiv.Implementation;

/// <summary>
/// Thin seam over the generated <see cref="SaksarkivClientV3"/> health probe, introduced so the
/// health check can be unit tested without the Kiota-generated client.
/// </summary>
/// <remarks>
/// The v3 API has no dedicated health endpoint, so a lightweight, unauthenticated-safe GET
/// (<c>/api/v3/metadata/tilgangskoder</c>) is used as the liveness probe until a version-agnostic
/// health endpoint becomes available.
/// </remarks>
internal interface ISaksarkivHealthPinger
{
    Task<string?> PongAsync(CancellationToken cancellationToken = default);
}

internal class SaksarkivHealthPinger(SaksarkivClientV3 saksarkivClient) : ISaksarkivHealthPinger
{
    public async Task<string?> PongAsync(CancellationToken cancellationToken = default)
    {
        var response = await saksarkivClient.Api.V3.Metadata.Tilgangskoder.GetAsync(
            cancellationToken: cancellationToken
        );

        return $"tilgangskoder: {response?.Count ?? 0}";
    }
}
