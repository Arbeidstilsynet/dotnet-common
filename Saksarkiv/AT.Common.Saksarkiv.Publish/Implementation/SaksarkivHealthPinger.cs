namespace Arbeidstilsynet.Common.Saksarkiv.Implementation;

/// <summary>
/// Thin seam over the generated <see cref="SaksarkivClient"/> health ping, introduced so the
/// health check can be unit tested without the Kiota-generated client.
/// </summary>
internal interface ISaksarkivHealthPinger
{
    Task<string?> PongAsync(CancellationToken cancellationToken = default);
}

internal class SaksarkivHealthPinger(SaksarkivClient saksarkivClient) : ISaksarkivHealthPinger
{
    public Task<string?> PongAsync(CancellationToken cancellationToken = default)
    {
        return saksarkivClient.Apiv2.Health.Pong.GetAsync(cancellationToken: cancellationToken);
    }
}
