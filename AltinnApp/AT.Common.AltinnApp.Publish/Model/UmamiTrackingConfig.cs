namespace Arbeidstilsynet.Common.AltinnApp.Model;

/// <summary>
/// Represents a sanitized Altinn page URL and title safe to send to Umami without instance or party identifiers.
/// </summary>
/// <param name="Url">The sanitized path, such as <c>/dat/forhandsmelding</c> or <c>/dat/forhandsmelding/5-Byggherre</c>.</param>
/// <param name="Title">The sanitized page title.</param>
public sealed record UmamiSanitizedPage(string Url, string Title);

/// <summary>
/// Resolved configuration for privacy-safe Umami browser tracking in an Altinn application.
/// </summary>
/// <param name="WebsiteId">The lowercase hyphenated Umami website UUID.</param>
/// <param name="CollectorOrigin">The HTTPS origin of the Umami collection gateway.</param>
/// <param name="AppOrigin">The expected HTTPS origin where the Altinn application is hosted.</param>
/// <param name="AppHostname">The hostname of <paramref name="AppOrigin"/>.</param>
/// <param name="AppPath">The application base path, such as <c>/dat/forhandsmelding</c>.</param>
/// <param name="AppTitle">The display title reported for the application.</param>
/// <param name="AllowedSteps">The ordered allowlist of form layout steps discovered from <c>Settings.json</c> or configuration.</param>
public sealed record UmamiTrackingConfig(
    string WebsiteId,
    Uri CollectorOrigin,
    Uri AppOrigin,
    string AppHostname,
    string AppPath,
    string AppTitle,
    IReadOnlyList<string> AllowedSteps
)
{
    /// <summary>
    /// Gets the full URL of the collector-hosted Altinn browser integration script (<c>/altinn.js</c>).
    /// </summary>
    public string AltinnScriptUrl => new Uri(CollectorOrigin, "/altinn.js").ToString();

    /// <summary>
    /// Gets the full URL of the upstream Umami tracker script (<c>/script.js</c>).
    /// </summary>
    public string ScriptUrl => new Uri(CollectorOrigin, "/script.js").ToString();

    /// <summary>
    /// Maps a raw Altinn browser pathname and optional hash to a sanitized page URL and title,
    /// stripping <c>partyId</c>, <c>instanceGuid</c>, task IDs, and query strings.
    /// </summary>
    /// <param name="pathname">The browser <c>window.location.pathname</c>.</param>
    /// <param name="hash">The optional browser <c>window.location.hash</c>.</param>
    /// <returns>The sanitized page metadata, or <see langword="null"/> if <paramref name="pathname"/> is outside <see cref="AppPath"/>.</returns>
    public UmamiSanitizedPage? ResolveSanitizedPage(string? pathname, string? hash = null)
    {
        if (
            string.IsNullOrWhiteSpace(pathname)
            || (
                !string.Equals(pathname, AppPath, StringComparison.Ordinal)
                && !pathname.StartsWith($"{AppPath}/", StringComparison.Ordinal)
            )
        )
        {
            return null;
        }

        var subPath = pathname[AppPath.Length..];
        var hashPath = (hash ?? string.Empty).TrimStart('#').Split('?')[0];
        var segments = $"{subPath}/{hashPath}".Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        for (var i = segments.Length - 1; i >= 0; i--)
        {
            var matchedStep = AllowedSteps.FirstOrDefault(step =>
                string.Equals(step, segments[i], StringComparison.OrdinalIgnoreCase)
            );
            if (matchedStep is not null)
            {
                return new UmamiSanitizedPage(
                    $"{AppPath}/{matchedStep}",
                    $"{AppTitle} - {matchedStep}"
                );
            }
        }

        return new UmamiSanitizedPage(AppPath, AppTitle);
    }
}
