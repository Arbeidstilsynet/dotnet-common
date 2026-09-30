using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for the Dialogporten API.
/// </summary>
public interface IAltinnDialogportenClient
{
    /// <summary>
    /// Looks up the dialog associated with an Altinn instance reference.
    /// </summary>
    Task<DialogportenLookupResponse> LookupDialog(
        string instanceRef,
        CancellationToken cancellationToken = default
    );
}
