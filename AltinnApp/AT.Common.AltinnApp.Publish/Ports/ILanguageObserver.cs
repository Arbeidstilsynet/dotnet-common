using Altinn.App.Core.Features;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Interface for observers that want to be notified when the language of the application changes.
/// Add an implementation with <see cref="DependencyInjection.DependencyInjectionExtensions.AddLanguageObserver{T}"/>
/// </summary>
public interface ILanguageObserver
{
    /// <summary>
    /// This is called on <see cref="IDataProcessor.ProcessDataRead"/> and does not guarantee that a change has occurred.
    /// </summary>
    /// <param name="dataModel">Current data model</param>
    /// <param name="language">Guaranteed to be non-empty. Two-letter ISO name</param>
    /// <returns></returns>
    Task NotifyCurrentLanguage(object dataModel, string language);
}
