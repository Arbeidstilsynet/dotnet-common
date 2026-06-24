namespace Arbeidstilsynet.Common.Saksarkiv.Ports;

public interface ISaksarkivTokenProvider
{
    Task<string> GetAccessToken(string scope, CancellationToken cancellationToken = default);
}
