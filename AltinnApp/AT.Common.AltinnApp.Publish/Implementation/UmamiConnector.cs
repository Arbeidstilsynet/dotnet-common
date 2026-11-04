using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal sealed partial class UmamiConnector : IUmamiConnector
{
    private readonly ILogger<UmamiConnector> _logger;

    public UmamiTrackingConfig? Config { get; }

    public UmamiConnector(
        IHostEnvironment environment,
        IConfiguration configuration,
        UmamiConfiguration? options = null,
        ILogger<UmamiConnector>? logger = null
    )
    {
        _logger = logger ?? NullLogger<UmamiConnector>.Instance;
        try
        {
            Config = ResolveConfig(environment, configuration, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Umami tracking is disabled because configuration could not be resolved"
            );
            Config = null;
        }
    }

    public IHtmlContent RenderScriptTag()
    {
        if (Config is not { } config)
        {
            return HtmlString.Empty;
        }

        try
        {
            var encoder = HtmlEncoder.Default;
            var steps = string.Join(",", config.AllowedSteps);
            var allowedOrigin = config.AppOrigin.GetLeftPart(UriPartial.Authority);

            return new HtmlString(
                $"<script async crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\" "
                    + $"src=\"{encoder.Encode(config.AltinnScriptUrl)}\" "
                    + $"data-website-id=\"{encoder.Encode(config.WebsiteId)}\" "
                    + $"data-allowed-origin=\"{encoder.Encode(allowedOrigin)}\" "
                    + $"data-app-path=\"{encoder.Encode(config.AppPath)}\" "
                    + $"data-title=\"{encoder.Encode(config.AppTitle)}\" "
                    + $"data-steps=\"{encoder.Encode(steps)}\"></script>"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render Umami script tag");
            return HtmlString.Empty;
        }
    }

    internal static UmamiTrackingConfig? ResolveConfig(
        IHostEnvironment environment,
        IConfiguration configuration,
        UmamiConfiguration? options = null
    )
    {
        if (string.IsNullOrWhiteSpace(environment.EnvironmentName))
        {
            return null;
        }

        var isDevelopment = environment.IsDevelopment();
        var enableInDevelopment =
            bool.TryParse(
                configuration[$"{UmamiConfiguration.SectionName}:EnableInDevelopment"],
                out var parsedEnable
            )
                ? parsedEnable
                : options?.EnableInDevelopment ?? false;

        if (isDevelopment && !enableInDevelopment)
        {
            return null;
        }

        var rawWebsiteId = FirstNonBlank(
            configuration[$"{UmamiConfiguration.SectionName}:WebsiteId"],
            configuration["UMAMI_WEBSITE_ID"],
            options?.WebsiteId
        );

        if (string.IsNullOrWhiteSpace(rawWebsiteId))
        {
            return null;
        }

        var websiteId = rawWebsiteId.Trim();
        if (
            !UuidPattern().IsMatch(websiteId)
            || !Guid.TryParseExact(websiteId, "D", out var parsedGuid)
            || parsedGuid == Guid.Empty
        )
        {
            throw new InvalidOperationException(
                "Umami:WebsiteId must be a valid Umami website UUID"
            );
        }

        var rawCollectorOrigin = FirstNonBlank(
            configuration[$"{UmamiConfiguration.SectionName}:CollectorOrigin"],
            options?.CollectorOrigin,
            UmamiConfiguration.DefaultCollectorOrigin
        )!;
        var collectorOrigin = ValidateOrigin(
            rawCollectorOrigin,
            $"{UmamiConfiguration.SectionName}:CollectorOrigin",
            allowHttp: isDevelopment
        );

        var defaultAppOrigin = isDevelopment
            ? UmamiConfiguration.DefaultDevelopmentAppOrigin
            : environment.IsProduction()
                ? UmamiConfiguration.DefaultProductionAppOrigin
                : UmamiConfiguration.DefaultStagingAppOrigin;
        var rawAppOrigin = FirstNonBlank(
            configuration[$"{UmamiConfiguration.SectionName}:AppOrigin"],
            options?.AppOrigin,
            defaultAppOrigin
        )!;
        var appOrigin = ValidateOrigin(
            rawAppOrigin,
            $"{UmamiConfiguration.SectionName}:AppOrigin",
            allowHttp: isDevelopment
        );

        var appPath = ResolveAppPath(environment.ContentRootPath, configuration, options);
        var appTitle = ResolveAppTitle(appPath, configuration, options);
        var allowedSteps = ResolveAllowedSteps(environment.ContentRootPath, configuration, options);

        return new UmamiTrackingConfig(
            WebsiteId: websiteId.ToLowerInvariant(),
            CollectorOrigin: collectorOrigin,
            AppOrigin: appOrigin,
            AppHostname: appOrigin.Host,
            AppPath: appPath,
            AppTitle: appTitle,
            AllowedSteps: allowedSteps
        );
    }

    private static string ResolveAppPath(
        string? contentRootPath,
        IConfiguration configuration,
        UmamiConfiguration? options
    )
    {
        var configured = FirstNonBlank(
            configuration[$"{UmamiConfiguration.SectionName}:AppPath"],
            options?.AppPath
        );
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return ValidateAppPath(configured);
        }

        if (!string.IsNullOrWhiteSpace(contentRootPath))
        {
            var metadataPath = Path.Combine(contentRootPath, "config", "applicationmetadata.json");
            if (File.Exists(metadataPath))
            {
                try
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(metadataPath));
                    if (
                        doc.RootElement.TryGetProperty("id", out var idProp)
                        && idProp.GetString() is { Length: > 0 } id
                    )
                    {
                        return ValidateAppPath(id);
                    }
                }
                catch (Exception ex) when (ex is not InvalidOperationException)
                {
                    // Fall back when applicationmetadata.json cannot be read.
                }
            }
        }

        throw new InvalidOperationException(
            "Unable to determine Altinn application path. Configure Umami:AppPath or ensure config/applicationmetadata.json contains a valid 'id' (org/app)."
        );
    }

    private static string ValidateAppPath(string rawPath)
    {
        var normalized = rawPath.Trim().TrimEnd('/');
        if (!normalized.StartsWith('/'))
        {
            normalized = $"/{normalized}";
        }

        if (!AppPathPattern().IsMatch(normalized))
        {
            throw new InvalidOperationException(
                "Umami:AppPath must be in the format '/<org>/<app>'"
            );
        }

        return normalized;
    }

    private static string ResolveAppTitle(
        string appPath,
        IConfiguration configuration,
        UmamiConfiguration? options
    )
    {
        var configured = FirstNonBlank(
            configuration[$"{UmamiConfiguration.SectionName}:AppTitle"],
            options?.AppTitle
        );
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var appSegment = appPath.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (string.IsNullOrWhiteSpace(appSegment))
        {
            return appPath.TrimStart('/');
        }

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(appSegment.ToLowerInvariant());
    }

    private static IReadOnlyList<string> ResolveAllowedSteps(
        string? contentRootPath,
        IConfiguration configuration,
        UmamiConfiguration? options
    )
    {
        var configuredSteps = configuration
            .GetSection($"{UmamiConfiguration.SectionName}:AllowedSteps")
            .GetChildren()
            .Select(section => section.Value?.Trim())
            .Where(step => !string.IsNullOrWhiteSpace(step))
            .Select(step => step!)
            .ToList();

        if (configuredSteps.Count > 0)
        {
            return configuredSteps;
        }

        if (options?.AllowedSteps is { Count: > 0 })
        {
            return options
                .AllowedSteps.Where(step => !string.IsNullOrWhiteSpace(step))
                .Select(step => step.Trim())
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(contentRootPath))
        {
            var uiDir = Path.Combine(contentRootPath, "ui");
            if (Directory.Exists(uiDir))
            {
                try
                {
                    var discovered = new List<string>();
                    foreach (
                        var settingsFile in Directory
                            .EnumerateFiles(uiDir, "Settings.json", SearchOption.AllDirectories)
                            .OrderBy(path => path, StringComparer.Ordinal)
                    )
                    {
                        using var doc = JsonDocument.Parse(File.ReadAllText(settingsFile));
                        if (
                            doc.RootElement.TryGetProperty("pages", out var pages)
                            && pages.TryGetProperty("order", out var order)
                            && order.ValueKind == JsonValueKind.Array
                        )
                        {
                            foreach (var item in order.EnumerateArray())
                            {
                                if (
                                    item.GetString() is { Length: > 0 } step
                                    && !discovered.Contains(step, StringComparer.OrdinalIgnoreCase)
                                )
                                {
                                    discovered.Add(step);
                                }
                            }
                        }
                    }

                    if (discovered.Count > 0)
                    {
                        return discovered;
                    }
                }
                catch
                {
                    // Fall back to empty list if Settings.json cannot be parsed.
                }
            }
        }

        return [];
    }

    private static string? FirstNonBlank(params string?[] candidates) =>
        candidates.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static Uri ValidateOrigin(string origin, string settingName, bool allowHttp = false)
    {
        if (
            !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            || (
                uri.Scheme != Uri.UriSchemeHttps
                && (!allowHttp || uri.Scheme != Uri.UriSchemeHttp)
            )
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment)
            || uri.AbsolutePath != "/"
            || !string.Equals(
                uri.GetLeftPart(UriPartial.Authority),
                origin,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new InvalidOperationException(
                $"{settingName} must be an exact HTTPS origin without path, query, fragment or credentials"
            );
        }

        return new Uri(uri.GetLeftPart(UriPartial.Authority));
    }

    [GeneratedRegex("^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$", RegexOptions.IgnoreCase)]
    private static partial Regex UuidPattern();

    [GeneratedRegex("^/[a-z0-9-]+/[a-z0-9-]+$", RegexOptions.IgnoreCase)]
    private static partial Regex AppPathPattern();
}
