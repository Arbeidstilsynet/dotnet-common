using Arbeidstilsynet.Common.AltinnApp.Model;
using Microsoft.AspNetCore.Html;

namespace Arbeidstilsynet.Common.AltinnApp.Ports;

/// <summary>
/// Provides Umami analytics configuration and renders the Umami tracker script tag for an Altinn application.
/// </summary>
public interface IUmamiConnector
{
    /// <summary>
    /// Gets the resolved Umami tracking configuration, or <see langword="null"/> when tracking is disabled
    /// (for example in <c>Development</c> or when <c>Umami:WebsiteId</c> is not configured).
    /// </summary>
    UmamiTrackingConfig? Config { get; }

    /// <summary>
    /// Renders the standard Umami tracker <c>&lt;script&gt;</c> element (<see cref="UmamiTrackingConfig.ScriptUrl"/>),
    /// restricted to the application hostname and excluding query strings, together with an inline
    /// <c>window.umamiConfig</c> script that masks party and instance IDs in app-frontend routes,
    /// or <see cref="HtmlString.Empty"/> when tracking is disabled.
    /// </summary>
    /// <returns>An <see cref="IHtmlContent"/> representing the script tags or empty content.</returns>
    IHtmlContent RenderScriptTag();

    /// <summary>
    /// Renders the same script tags as <see cref="RenderScriptTag()"/>, with a <c>nonce</c> attribute for apps whose
    /// Content Security Policy only allows scripts with a nonce.
    /// </summary>
    /// <param name="nonce">The Content Security Policy nonce for the current response, or <see langword="null"/> to omit it.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the script tags or empty content.</returns>
    IHtmlContent RenderScriptTag(string? nonce) => RenderScriptTag();
}
