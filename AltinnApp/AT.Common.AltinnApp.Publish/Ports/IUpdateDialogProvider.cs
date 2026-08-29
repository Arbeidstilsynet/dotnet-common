using Arbeidstilsynet.Common.AltinnApp.Model;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Interface for supplying the update-specific dialog logic to the UpdateDialog task.
/// </summary>
/// <typeparam name="TDataModel">The skjema data model type related to the instance.</typeparam>
public interface IUpdateDialogProvider<TDataModel>
    where TDataModel : class
{
    /// <summary>
    /// Decides whether an existing Altinn dialog should be reused or a new dialog should be
    /// created for the given model.
    /// </summary>
    /// <param name="model">The submitted skjema model.</param>
    /// <param name="dialogId">
    /// A pre-generated dialog id (UUIDv7) to use when a new dialog is created. Use this as the
    /// <c>Id</c> of the returned <see cref="DialogResolution.CreateNew"/> request so the task can
    /// reference the created dialog afterwards.
    /// </param>
    /// <returns>The resolution describing how to obtain the dialog.</returns>
    DialogResolution Resolve(TDataModel model, Guid dialogId);
}
