namespace Arbeidstilsynet.Common.EraClient.Ports.Model;

public record AuthenticationRequestDto
{
    /// <summary>
    /// OAuth ClientId
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// OAuth ClientSecret
    /// </summary>
    public required string ClientSecret { get; init; }
}
