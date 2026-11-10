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
/// <param name="CollectorOrigin">The HTTPS origin of the Umami instance or collection gateway.</param>
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
    /// Gets the event name emitted when a form instance enters its first step. Defaults to <c>"skjema-start"</c>.
    /// Set to <c>""</c> to disable the start event.
    /// </summary>
    public string StartEventName { get; init; } = "skjema-start";

    /// <summary>
    /// Gets the event name emitted when the Altinn process completes (<c>ProcessState.ended</c>). Defaults to <c>"skjema-innsendt"</c>.
    /// Set to <c>""</c> to disable the submission event.
    /// </summary>
    public string SubmissionEventName { get; init; } = "skjema-innsendt";

    /// <summary>
    /// Gets the event name emitted when an Altinn API request returns HTTP 5xx. Defaults to <c>"skjema-feil"</c>.
    /// Set to <c>""</c> to disable the error event.
    /// </summary>
    public string ErrorEventName { get; init; } = "skjema-feil";

    /// <summary>
    /// Gets static metadata properties merged into the <c>data</c> payload of every custom event emitted by this application.
    /// </summary>
    public IReadOnlyDictionary<string, string>? AdditionalProperties { get; init; }

    /// <summary>
    /// Gets the absolute path of the Umami tracker script on <see cref="CollectorOrigin"/>. Defaults to <c>/script.js</c>.
    /// The tracker sends events to <c>api/send</c> relative to this path, for example <c>/public/api/send</c> for
    /// <c>/public/tracker.js</c>.
    /// </summary>
    public string ScriptPath { get; init; } = "/script.js";

    /// <summary>
    /// Gets the full URL of the Umami tracker script (<see cref="CollectorOrigin"/> + <see cref="ScriptPath"/>).
    /// </summary>
    public string ScriptUrl => new Uri(CollectorOrigin, ScriptPath).ToString();

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
