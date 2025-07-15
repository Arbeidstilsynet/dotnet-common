namespace Arbeidstilsynet.Common.Altinn.Ports;

public interface IAltinnTokenProvider
{
    Task<string> GetToken();
}