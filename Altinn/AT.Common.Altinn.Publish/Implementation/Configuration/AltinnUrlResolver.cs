using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Configuration;

/// <summary>
/// The fully resolved base URL for every Altinn API the package talks to.
/// </summary>
/// <remarks>
/// Each URL is the base the corresponding client is configured with, and already accounts for the
/// differing base-path conventions between the OpenAPI specifications: the storage and events specs
/// declare an <c>/api/v1</c> server suffix while their paths are relative, the correspondence spec
/// declares a bare host with the prefix baked into every path, and Dialogporten sits between the two.
/// </remarks>
internal record ResolvedAltinnUrls
{
    public required Uri AuthenticationUrl { get; init; }
    public required Uri StorageUrl { get; init; }
    public required Uri EventsUrl { get; init; }
    public required Uri CorrespondenceUrl { get; init; }
    public required Uri DialogportenUrl { get; init; }
    public required Uri AppBaseUrl { get; init; }
    public required Uri MaskinportenUrl { get; init; }
}

internal record AltinnResolution
{
    public required AltinnEnvironment Target { get; init; }
    public required ResolvedAltinnUrls Urls { get; init; }

    /// <summary>
    /// Names of the URLs whose resolved value differs from the target's default, i.e. the overrides
    /// that actually took effect.
    /// </summary>
    public required IReadOnlyList<string> EffectiveOverrides { get; init; }
}

internal static class AltinnUrlResolver
{
    private const string AuthenticationBasePath = "authentication/api/v1";
    private const string StorageBasePath = "storage/api/v1";
    private const string EventsBasePath = "events/api/v1";
    private const string DialogportenBasePath = "dialogporten";

    // The correspondence specification declares a bare host as its server and prefixes every path
    // with /correspondence/api/v1, so no base path is appended here.
    private const string CorrespondenceBasePath = "";

    public static AltinnResolution Resolve(
        IWebHostEnvironment hostEnvironment,
        AltinnConfiguration configuration
    )
    {
        var target = ResolveTarget(hostEnvironment, configuration);

        var defaults = BuildUrls(target, configuration.OrgId, overrides: null);
        var resolved = BuildUrls(target, configuration.OrgId, configuration.Overrides);

        var effectiveOverrides = DiffUrls(defaults, resolved);

        if (hostEnvironment.IsProduction() && effectiveOverrides.Count > 0)
        {
            throw new InvalidOperationException(
                "The host environment is Production, so overriding Altinn base URLs is not permitted. "
                    + $"Offending override(s): {string.Join(", ", effectiveOverrides)}. "
                    + "Remove the override, or run the application in a non-production environment."
            );
        }

        return new AltinnResolution
        {
            Target = target,
            Urls = resolved,
            EffectiveOverrides = effectiveOverrides,
        };
    }

    private static AltinnEnvironment ResolveTarget(
        IWebHostEnvironment hostEnvironment,
        AltinnConfiguration configuration
    )
    {
        if (hostEnvironment.IsProduction())
        {
            if (
                configuration.Environment is { } requested
                && requested != AltinnEnvironment.Production
            )
            {
                throw new InvalidOperationException(
                    "The host environment is Production, so the Altinn environment must be "
                        + $"{nameof(AltinnEnvironment.Production)}, but {requested} was requested. "
                        + "Production applications must not target a test instance of Altinn."
                );
            }

            return AltinnEnvironment.Production;
        }

        if (hostEnvironment.IsStaging())
        {
            return configuration.Environment ?? AltinnEnvironment.Tt02;
        }

        return configuration.Environment
            ?? throw new InvalidOperationException(
                $"No Altinn environment configured for host environment '{hostEnvironment.EnvironmentName}'. "
                    + $"Set {nameof(AltinnConfiguration)}.{nameof(AltinnConfiguration.Environment)} to "
                    + $"{nameof(AltinnEnvironment.Tt02)} to target the Altinn test instance, or "
                    + $"{nameof(AltinnEnvironment.Local)} to target a locally running Altinn instance. "
                    + "Only the Production and Staging host environments have a default."
            );
    }

    private static ResolvedAltinnUrls BuildUrls(
        AltinnEnvironment target,
        string orgId,
        AltinnUrlOverrides? overrides
    )
    {
        var platformUrl = overrides?.PlatformUrl ?? GetPlatformUrl(target);

        return new ResolvedAltinnUrls
        {
            AuthenticationUrl =
                overrides?.AuthenticationUrl ?? Combine(platformUrl, AuthenticationBasePath),
            StorageUrl = overrides?.StorageUrl ?? Combine(platformUrl, StorageBasePath),
            EventsUrl = overrides?.EventsUrl ?? Combine(platformUrl, EventsBasePath),
            CorrespondenceUrl =
                overrides?.CorrespondenceUrl ?? Combine(platformUrl, CorrespondenceBasePath),
            DialogportenUrl =
                overrides?.DialogportenUrl ?? Combine(platformUrl, DialogportenBasePath),
            AppBaseUrl = overrides?.AppBaseUrl ?? GetAppBaseUrl(target, orgId),
            MaskinportenUrl = overrides?.MaskinportenUrl ?? GetMaskinportenUrl(target),
        };
    }

    private static IReadOnlyList<string> DiffUrls(
        ResolvedAltinnUrls defaults,
        ResolvedAltinnUrls resolved
    )
    {
        var differences = new List<string>();

        foreach (var property in typeof(ResolvedAltinnUrls).GetProperties())
        {
            if (!Equals(property.GetValue(defaults), property.GetValue(resolved)))
            {
                differences.Add(property.Name);
            }
        }

        return differences;
    }

    private static Uri GetPlatformUrl(AltinnEnvironment target) =>
        target switch
        {
            AltinnEnvironment.Production => new Uri("https://platform.altinn.no/"),
            AltinnEnvironment.Tt02 => new Uri("https://platform.tt02.altinn.no/"),
            AltinnEnvironment.Local => new Uri("http://local.altinn.cloud:8000/"),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static Uri GetAppBaseUrl(AltinnEnvironment target, string orgId) =>
        target switch
        {
            AltinnEnvironment.Production => new Uri($"https://{orgId}.apps.altinn.no/"),
            AltinnEnvironment.Tt02 => new Uri($"https://{orgId}.apps.tt02.altinn.no/"),
            AltinnEnvironment.Local => new Uri("http://local.altinn.cloud:8000/"),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    private static Uri GetMaskinportenUrl(AltinnEnvironment target) =>
        target switch
        {
            AltinnEnvironment.Production => new Uri("https://maskinporten.no/"),
            AltinnEnvironment.Tt02 or AltinnEnvironment.Local => new Uri(
                "https://test.maskinporten.no/"
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null),
        };

    /// <summary>
    /// Appends an API base path to a base URL, preserving any path already present on the base URL
    /// so that overrides pointing at a sub-path (e.g. a mock server mounted under /altinn) work.
    /// </summary>
    private static Uri Combine(Uri baseUrl, string basePath)
    {
        var trimmed = baseUrl.ToString().TrimEnd('/');

        return string.IsNullOrEmpty(basePath) ? new Uri(trimmed) : new Uri($"{trimmed}/{basePath}");
    }
}
