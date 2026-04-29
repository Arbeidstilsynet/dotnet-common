using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for interacting with Dialogporten.
/// </summary>
public interface IAltinnDialogportenClient
{
    /// <summary>
    /// Looks up a dialog by instance reference in service owner context.
    /// </summary>
    /// <param name="instanceRef">The instance reference to look up.</param>
    /// <returns>The dialog lookup metadata.</returns>
    Task<DialogportenLookupResponse> LookupDialog(string instanceRef);
}
