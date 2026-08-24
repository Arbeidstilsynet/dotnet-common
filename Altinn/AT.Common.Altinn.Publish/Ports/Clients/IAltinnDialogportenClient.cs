using Arbeidstilsynet.Common.Altinn.Dialogporten.Models;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for the Dialogporten API.
/// </summary>
public interface IAltinnDialogportenClient
{
    /// <summary>
    /// Looks up the dialog associated with an Altinn instance reference.
    /// </summary>
    Task<V1CommonIdentifierLookup_ServiceOwnerIdentifierLookup> LookupDialog(
        string instanceRef,
        CancellationToken cancellationToken = default
    );
}
