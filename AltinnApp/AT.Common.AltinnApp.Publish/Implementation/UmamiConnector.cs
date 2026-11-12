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

        var encoder = HtmlEncoder.Default;
        return new HtmlString(
            $"<script defer src=\"{encoder.Encode(config.ScriptUrl)}\" "
                + $"data-host-url=\"{encoder.Encode(config.CollectorOrigin.ToString())}\" "
                + $"data-website-id=\"{encoder.Encode(config.WebsiteId)}\" "
                + $"data-domains=\"{encoder.Encode(config.AppHostname)}\" "
                + "data-exclude-search=\"true\" data-exclude-hash=\"true\" data-do-not-track=\"true\" data-errors=\"true\"></script>"
        );
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
        if (isDevelopment && !IsEnabledInDevelopment(configuration, options))
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
            configuration["UMAMI_COLLECTOR_ORIGIN"],
            options?.CollectorOrigin
        );
        if (string.IsNullOrWhiteSpace(rawCollectorOrigin))
        {
            throw new InvalidOperationException(
                "Umami:CollectorOrigin must be configured when Umami:WebsiteId is set"
            );
        }

        var collectorOrigin = ValidateOrigin(
            rawCollectorOrigin,
            $"{UmamiConfiguration.SectionName}:CollectorOrigin",
            allowHttp: isDevelopment
        );

        var scriptPath = ValidateScriptPath(
            FirstNonBlank(
                configuration[$"{UmamiConfiguration.SectionName}:ScriptPath"],
                options?.ScriptPath
            ) ?? UmamiConfiguration.DefaultScriptPath
        );

        var defaultAppOrigin = GetDefaultAppOrigin(environment);
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

        return new UmamiTrackingConfig(
            WebsiteId: websiteId.ToLowerInvariant(),
            CollectorOrigin: collectorOrigin,
            AppOrigin: appOrigin,
            AppHostname: appOrigin.Host,
            AppPath: appPath,
            AppTitle: appTitle
        )
        {
            ScriptPath = scriptPath,
        };
    }

    private static bool IsEnabledInDevelopment(
        IConfiguration configuration,
        UmamiConfiguration? options
    )
    {
        if (
            bool.TryParse(
                configuration[$"{UmamiConfiguration.SectionName}:EnableInDevelopment"],
                out var parsedEnable
            )
        )
        {
            return parsedEnable;
        }

        return options?.EnableInDevelopment ?? false;
    }

    private static string GetDefaultAppOrigin(IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            return new UriBuilder(Uri.UriSchemeHttp, "local.altinn.cloud", 8000).Uri.GetLeftPart(
                UriPartial.Authority
            );
        }

        if (environment.IsProduction())
        {
            return UmamiConfiguration.DefaultProductionAppOrigin;
        }

        return UmamiConfiguration.DefaultStagingAppOrigin;
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

    private static string? FirstNonBlank(params string?[] candidates) =>
        candidates.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static Uri ValidateOrigin(string origin, string settingName, bool allowHttp = false)
    {
        if (
            !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && (!allowHttp || uri.Scheme != Uri.UriSchemeHttp))
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

    private static string ValidateScriptPath(string scriptPath)
    {
        if (!ScriptPathPattern().IsMatch(scriptPath))
        {
            throw new InvalidOperationException(
                $"{UmamiConfiguration.SectionName}:ScriptPath must be an absolute path such as /script.js, without query, fragment or dot segments"
            );
        }

        return scriptPath;
    }

    [GeneratedRegex("^(?:/[A-Za-z0-9_~-][A-Za-z0-9._~-]*)+$")]
    private static partial Regex ScriptPathPattern();

    [GeneratedRegex("^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$", RegexOptions.IgnoreCase)]
    private static partial Regex UuidPattern();

    [GeneratedRegex("^/[a-z0-9-]+/[a-z0-9-]+$", RegexOptions.IgnoreCase)]
    private static partial Regex AppPathPattern();
}
