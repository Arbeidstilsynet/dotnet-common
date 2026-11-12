namespace Arbeidstilsynet.Common.AltinnApp.Model;

/// <summary>
/// Resolved configuration for Umami tracking in an Altinn application.
/// </summary>
/// <param name="WebsiteId">The lowercase hyphenated Umami website UUID.</param>
/// <param name="CollectorOrigin">The HTTPS origin of the Umami instance or collection gateway.</param>
/// <param name="AppOrigin">The expected HTTPS origin where the Altinn application is hosted.</param>
/// <param name="AppHostname">The hostname of <paramref name="AppOrigin"/>.</param>
/// <param name="AppPath">The application base path, such as <c>/dat/forhandsmelding</c>.</param>
/// <param name="AppTitle">The display title reported for the application.</param>
public sealed record UmamiTrackingConfig(
    string WebsiteId,
    Uri CollectorOrigin,
    Uri AppOrigin,
    string AppHostname,
    string AppPath,
    string AppTitle
)
{
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
    /// Gets the Umami host URL used by the tracker (<c>data-host-url</c>): the directory of <see cref="ScriptUrl"/>
    /// without trailing slash, for example <c>https://umami.example.com/public</c> for <c>/public/tracker.js</c>.
    /// The tracker sends events to <c>{HostUrl}/api/send</c>.
    /// </summary>
    public string HostUrl => new Uri(new Uri(ScriptUrl), ".").ToString().TrimEnd('/');

    /// <summary>
    /// Gets the Umami collection endpoint (<c>api/send</c> relative to <see cref="ScriptUrl"/>).
    /// </summary>
    public Uri SendUrl => new(new Uri(ScriptUrl), "api/send");
}
