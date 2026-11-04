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
    /// Renders the <c>&lt;script&gt;</c> element for the collector-hosted Altinn tracker (<c>/altinn.js</c>)
    /// when tracking is active, or <see cref="HtmlString.Empty"/> when tracking is disabled.
    /// </summary>
    /// <returns>An <see cref="IHtmlContent"/> representing the script tag or empty content.</returns>
    IHtmlContent RenderScriptTag();
}
