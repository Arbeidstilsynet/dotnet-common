using Arbeidstilsynet.Common.AltinnApp.Model;
using Microsoft.AspNetCore.Html;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Provides sanitized Umami analytics configuration and renders the browser tracking script tag for an Altinn application.
/// </summary>
public interface IUmamiConnector
{
    /// <summary>
    /// Gets the resolved Umami tracking configuration, or <see langword="null"/> when tracking is disabled
    /// (for example in <c>Development</c> or when <c>Umami:WebsiteId</c> is not configured).
    /// </summary>
    UmamiTrackingConfig? Config { get; }

    /// <summary>
    /// Renders the self-contained <c>&lt;script&gt;</c> element that loads the Umami collector script (<c>/script.js</c>)
    /// and tracks sanitized page views, form steps, completed submissions (<c>skjema-innsendt</c>), and 5xx errors (<c>skjema-feil</c>),
    /// or <see cref="HtmlString.Empty"/> when tracking is disabled.
    /// </summary>
    /// <returns>An <see cref="IHtmlContent"/> representing the script tag or empty content.</returns>
    IHtmlContent RenderScriptTag();
}
