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

            var websiteIdJson = SerializeForInlineScript(config.WebsiteId);
            var scriptUrlJson = SerializeForInlineScript(config.ScriptUrl);
            var allowedOriginJson = SerializeForInlineScript(allowedOrigin);
            var hostnameJson = SerializeForInlineScript(config.AppHostname);
            var appPathJson = SerializeForInlineScript(config.AppPath);
            var appTitleJson = SerializeForInlineScript(config.AppTitle);
            var allowedStepsJson = SerializeForInlineScript(config.AllowedSteps);
            var startEventNameJson = SerializeForInlineScript(config.StartEventName);
            var submissionEventNameJson = SerializeForInlineScript(config.SubmissionEventName);
            var errorEventNameJson = SerializeForInlineScript(config.ErrorEventName);
            var additionalPropsJson = SerializeForInlineScript(
                config.AdditionalProperties ?? new Dictionary<string, string>()
            );

            var script = $$"""
<script data-website-id="{{encoder.Encode(
                    config.WebsiteId
                )}}" data-allowed-origin="{{encoder.Encode(
                    allowedOrigin
                )}}" data-app-path="{{encoder.Encode(
                    config.AppPath
                )}}" data-title="{{encoder.Encode(config.AppTitle)}}" data-steps="{{encoder.Encode(
                    steps
                )}}">
(() => {
  try {
    const websiteId = {{websiteIdJson}};
    const scriptUrl = {{scriptUrlJson}};
    const allowedOrigin = {{allowedOriginJson}};
    const hostname = {{hostnameJson}};
    const appPath = {{appPathJson}};
    const pageTitle = {{appTitleJson}};
    const allowedSteps = {{allowedStepsJson}};
    const startEventName = {{startEventNameJson}};
    const submissionEventName = {{submissionEventNameJson}};
    const errorEventName = {{errorEventNameJson}};
    const staticProps = {{additionalPropsJson}};

    if (window.location.origin !== allowedOrigin) return;

    const stepLookup = new Map(allowedSteps.map((step) => [step.toLowerCase(), step]));
    const instanceRoutePattern = /\/instance\/([^/]+\/[^/?#]+)/i;

    function resolveInstanceKey(pathname, hash) {
      const combined = `${pathname || ""}/${(hash || "").replace(/^#/, "")}`;
      const match = combined.match(instanceRoutePattern);
      return match ? match[1].toLowerCase() : null;
    }

    function resolvePage(pathname, hash) {
      if (pathname !== appPath && !pathname.startsWith(`${appPath}/`)) return null;
      const subPath = pathname.slice(appPath.length);
      const hashPath = (hash || "").replace(/^#/, "").split("?")[0];
      const segments = `${subPath}/${hashPath}`.split("/").filter(Boolean);
      for (let i = segments.length - 1; i >= 0; i--) {
        const matchedStep = stepLookup.get(segments[i].toLowerCase());
        if (matchedStep) {
          return {
            url: `${appPath}/${matchedStep}`,
            title: `${pageTitle} - ${matchedStep}`,
            step: matchedStep,
          };
        }
      }
      return { url: appPath, title: pageTitle, step: "root" };
    }

    function classifyEndpoint(pathname) {
      const subPath = pathname.slice(appPath.length);
      if (/\/process(?:\/next)?$/i.test(subPath)) return "process";
      if (/\/validate$/i.test(subPath)) return "validate";
      if (/\/data(?:\/|$)/i.test(subPath)) return "data";
      if (/\/options\//i.test(subPath)) return "options";
      if (/\/datalist\//i.test(subPath)) return "datalist";
      if (/\/instances\/?$/i.test(subPath)) return "instantiation";
      return "other";
    }

    let trackerPromise;
    function loadTracker() {
      if (trackerPromise) return trackerPromise;
      trackerPromise = new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = scriptUrl;
        script.async = true;
        script.crossOrigin = "anonymous";
        script.dataset.websiteId = websiteId;
        script.dataset.autoTrack = "false";
        script.dataset.doNotTrack = "true";
        script.dataset.excludeSearch = "true";
        script.dataset.excludeHash = "true";
        script.dataset.domains = hostname;
        script.referrerPolicy = "no-referrer";
        script.addEventListener(
          "load",
          () => {
            if (window.umami && typeof window.umami.track === "function") resolve(window.umami);
            else reject(new Error("Umami script did not initialize its tracking API"));
          },
          { once: true }
        );
        script.addEventListener(
          "error",
          () => {
            script.remove();
            trackerPromise = undefined;
            reject(new Error("Unable to load the Umami collection script"));
          },
          { once: true }
        );
        document.head.append(script);
      });
      return trackerPromise;
    }

    function sendEvent(page, eventName, eventData) {
      const mergedData = eventName
        ? { ...staticProps, ...(eventData || {}) }
        : eventData;
      const hasData = mergedData && Object.keys(mergedData).length > 0;
      const payload = {
        website: websiteId,
        hostname,
        url: page.url,
        title: page.title,
        referrer: "",
        screen: `${window.screen?.width ?? 0}x${window.screen?.height ?? 0}`,
        language: window.navigator?.language ?? "",
        ...(eventName ? { name: eventName } : {}),
        ...(hasData ? { data: mergedData } : {}),
      };
      void loadTracker()
        .then((tracker) => Promise.resolve().then(() => tracker.track(payload)))
        .catch(() => {});
    }

    function currentSanitizedPage() {
      return resolvePage(window.location?.pathname ?? "", window.location?.hash ?? "") ?? {
        url: appPath,
        title: pageTitle,
        step: "root",
      };
    }

    let lastTrackedUrl = null;
    const startedInstances = new Set();
    function trackCurrentPage() {
      try {
        const pathname = window.location?.pathname ?? "";
        const hash = window.location?.hash ?? "";
        const page = resolvePage(pathname, hash);
        if (!page || page.url === lastTrackedUrl) return;
        lastTrackedUrl = page.url;
        sendEvent(page);
        if (page.step !== "root" && startEventName) {
          const instanceKey = resolveInstanceKey(pathname, hash) ?? "stateless";
          if (!startedInstances.has(instanceKey)) {
            startedInstances.add(instanceKey);
            sendEvent(page, startEventName, { status: "started", step: page.step });
          }
        }
      } catch {}
    }

    if (window.history) {
      for (const method of ["pushState", "replaceState"]) {
        const original = window.history[method];
        if (typeof original === "function") {
          window.history[method] = function (...args) {
            const result = original.apply(this, args);
            trackCurrentPage();
            return result;
          };
        }
      }
    }

    window.addEventListener("popstate", trackCurrentPage);
    window.addEventListener("hashchange", trackCurrentPage);

    let lastFocusedElementId = null;
    function captureElementId(element) {
      try {
        if (!element || typeof element !== "object") return null;
        if (typeof element.id === "string" && element.id.trim().length > 0) {
          return element.id.trim().slice(0, 128);
        }
        const closestWithId =
          typeof element.closest === "function" ? element.closest("[id]") : null;
        if (
          closestWithId &&
          typeof closestWithId.id === "string" &&
          closestWithId.id.trim().length > 0 &&
          closestWithId.id !== "root"
        ) {
          return closestWithId.id.trim().slice(0, 128);
        }
      } catch {}
      return null;
    }

    document.addEventListener(
      "focusin",
      (event) => {
        const id = captureElementId(event?.target);
        if (id) lastFocusedElementId = id;
      },
      true
    );

    function currentActiveElementId() {
      const current = captureElementId(document.activeElement);
      if (current && current !== "root") return current;
      return lastFocusedElementId;
    }

    function extractChangedFields(requestBody) {
      try {
        if (typeof requestBody !== "string" || (!requestBody.trim().startsWith("{") && !requestBody.trim().startsWith("["))) {
          return null;
        }
        const parsed = JSON.parse(requestBody);
        const patches = Array.isArray(parsed)
          ? parsed
          : Array.isArray(parsed?.patches)
            ? parsed.patches
            : Array.isArray(parsed?.patch)
              ? parsed.patch
              : null;
        if (!patches) return null;
        const paths = [];
        for (const entry of patches) {
          const opList = Array.isArray(entry?.patch) ? entry.patch : [entry];
          for (const op of opList) {
            if (typeof op?.path === "string" && op.path.startsWith("/")) {
              const sanitizedPath = op.path
                .slice(1)
                .replace(/\/\d+(?=\/|$)/g, "[]")
                .replaceAll("/", ".");
              if (sanitizedPath && !paths.includes(sanitizedPath)) {
                paths.push(sanitizedPath);
              }
            }
          }
        }
        return paths.length > 0 ? paths.slice(0, 10) : null;
      } catch {
        return null;
      }
    }

    function extractTraceId(xhr) {
      try {
        const body =
          xhr.response && typeof xhr.response === "object"
            ? xhr.response
            : typeof xhr.responseText === "string" && xhr.responseText.trim().startsWith("{")
              ? JSON.parse(xhr.responseText)
              : null;
        if (typeof body?.traceId === "string" && body.traceId.trim().length > 0) {
          return body.traceId.trim().slice(0, 128);
        }
        if (typeof xhr.getResponseHeader === "function") {
          for (const header of ["x-trace-id", "traceparent", "x-correlation-id", "request-id"]) {
            const value = xhr.getResponseHeader(header);
            if (typeof value === "string" && value.trim().length > 0) {
              return value.trim().slice(0, 128);
            }
          }
        }
      } catch {}
      return null;
    }

    function extractErrorHeader(xhr, headerName) {
      try {
        if (typeof xhr.getResponseHeader === "function") {
          const header = xhr.getResponseHeader(headerName);
          if (typeof header === "string" && header.trim().length > 0) {
            return header.trim().slice(0, 160);
          }
        }
      } catch {}
      return null;
    }

    function extractJsonHeader(xhr, headerName) {
      try {
        if (typeof xhr.getResponseHeader === "function") {
          const header = xhr.getResponseHeader(headerName);
          if (typeof header === "string" && header.trim().length > 0) {
            return JSON.parse(header);
          }
        }
      } catch {}
      return null;
    }

    const Xhr = window.XMLHttpRequest;
    if (typeof Xhr === "function" && Xhr.prototype) {
      const requests = new WeakMap();
      const submittedInstances = new Set();
      const processPattern = new RegExp(
        `^${appPath.replaceAll("/", "\\/")}\\/instances\\/([^/]+\\/[^/]+)\\/process(?:\\/next)?$`,
        "i"
      );
      const originalOpen = Xhr.prototype.open;
      const originalSend = Xhr.prototype.send;

      function isProcessEnded(xhr) {
        try {
          const body =
            xhr.response && typeof xhr.response === "object"
              ? xhr.response
              : typeof xhr.responseText === "string" && xhr.responseText.trim().startsWith("{")
                ? JSON.parse(xhr.responseText)
                : null;
          return typeof body?.ended === "string" && body.ended.trim().length > 0;
        } catch {
          return false;
        }
      }

      if (typeof originalOpen === "function" && typeof originalSend === "function") {
        Xhr.prototype.open = function (method, url, ...rest) {
          try {
            requests.set(this, {
              method: String(method ?? "GET").toUpperCase(),
              url: String(url ?? ""),
            });
          } catch {}
          return originalOpen.call(this, method, url, ...rest);
        };

        Xhr.prototype.send = function (...args) {
          try {
            const info = requests.get(this);
            if (info && typeof this.addEventListener === "function") {
              info.activeElement = currentActiveElementId();
              info.changedFields = extractChangedFields(args[0]);
              this.addEventListener(
                "loadend",
                () => {
                  try {
                    const target = new URL(info.url, allowedOrigin);
                    if (
                      target.origin !== allowedOrigin ||
                      (target.pathname !== appPath && !target.pathname.startsWith(`${appPath}/`))
                    ) {
                      return;
                    }
                    const status = Number(this.status ?? 0);
                    const page = currentSanitizedPage();
                    const customProps = extractJsonHeader(this, "x-umami-props");
                    const customEvents = extractJsonHeader(this, "x-umami-events");
                    if (Array.isArray(customEvents)) {
                      for (const evt of customEvents) {
                        if (typeof evt?.name === "string" && evt.name.trim().length > 0) {
                          sendEvent(page, evt.name.trim(), {
                            step: page.step,
                            ...(customProps && typeof customProps === "object" ? customProps : {}),
                            ...(evt.data && typeof evt.data === "object" ? evt.data : {}),
                          });
                        }
                      }
                    }
                    if (status >= 500) {
                      if (errorEventName) {
                        const activeElement = info.activeElement ?? currentActiveElementId();
                        const traceId = extractTraceId(this);
                        const errorType = extractErrorHeader(this, "x-error-type");
                        const errorLocation = extractErrorHeader(this, "x-error-location");
                        sendEvent(page, errorEventName, {
                          status: "error",
                          step: page.step,
                          httpMethod: info.method,
                          httpStatus: String(status),
                          endpoint: classifyEndpoint(target.pathname),
                          ...(activeElement ? { activeElement } : {}),
                          ...(info.changedFields ? { changedFields: info.changedFields } : {}),
                          ...(errorType ? { errorType } : {}),
                          ...(errorLocation ? { errorLocation } : {}),
                          ...(traceId ? { traceId } : {}),
                          ...(customProps && typeof customProps === "object" ? customProps : {}),
                        });
                      }
                      return;
                    }
                    const match = target.pathname.match(processPattern);
                    if (
                      match &&
                      submissionEventName &&
                      ["GET", "PUT", "POST"].includes(info.method) &&
                      status >= 200 &&
                      status < 300 &&
                      isProcessEnded(this)
                    ) {
                      const instanceKey = match[1].toLowerCase();
                      if (!submittedInstances.has(instanceKey)) {
                        submittedInstances.add(instanceKey);
                        sendEvent(page, submissionEventName, {
                          status: "success",
                          step: page.step,
                          ...(customProps && typeof customProps === "object" ? customProps : {}),
                        });
                      }
                    }
                  } catch {}
                },
                { once: true }
              );
            }
          } catch {}
          return originalSend.apply(this, args);
        };
      }
    }

    trackCurrentPage();
  } catch {}
})();
</script>
""";

            return new HtmlString(script);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render Umami script tag");
            return HtmlString.Empty;
        }
    }

    private static string SerializeForInlineScript<T>(T value) =>
        JsonSerializer.Serialize(value).Replace("</", "<\\/", StringComparison.Ordinal);

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
        var allowedSteps = ResolveAllowedSteps(environment.ContentRootPath, configuration, options);
        var startEventName = ResolveEventName(
            configuration[$"{UmamiConfiguration.SectionName}:StartEventName"],
            options?.StartEventName,
            "skjema-start"
        );
        var submissionEventName = ResolveEventName(
            configuration[$"{UmamiConfiguration.SectionName}:SubmissionEventName"],
            options?.SubmissionEventName,
            "skjema-innsendt"
        );
        var errorEventName = ResolveEventName(
            configuration[$"{UmamiConfiguration.SectionName}:ErrorEventName"],
            options?.ErrorEventName,
            "skjema-feil"
        );
        var additionalProperties = ResolveAdditionalProperties(configuration, options);

        return new UmamiTrackingConfig(
            WebsiteId: websiteId.ToLowerInvariant(),
            CollectorOrigin: collectorOrigin,
            AppOrigin: appOrigin,
            AppHostname: appOrigin.Host,
            AppPath: appPath,
            AppTitle: appTitle,
            AllowedSteps: allowedSteps
        )
        {
            StartEventName = startEventName,
            SubmissionEventName = submissionEventName,
            ErrorEventName = errorEventName,
            AdditionalProperties = additionalProperties,
        };
    }

    private static string ResolveEventName(
        string? configuredValue,
        string? optionValue,
        string defaultName
    )
    {
        if (configuredValue is not null)
        {
            return configuredValue.Trim();
        }

        if (optionValue is not null)
        {
            return optionValue.Trim();
        }

        return defaultName;
    }

    private static IReadOnlyDictionary<string, string>? ResolveAdditionalProperties(
        IConfiguration configuration,
        UmamiConfiguration? options
    )
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        if (options?.AdditionalProperties is not null)
        {
            foreach (var (key, value) in options.AdditionalProperties)
            {
                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    merged[key.Trim()] = value.Trim();
                }
            }
        }

        foreach (
            var child in configuration
                .GetSection($"{UmamiConfiguration.SectionName}:AdditionalProperties")
                .GetChildren()
                .Where(child =>
                    !string.IsNullOrWhiteSpace(child.Key) && !string.IsNullOrWhiteSpace(child.Value)
                )
        )
        {
            merged[child.Key.Trim()] = child.Value!.Trim();
        }

        return merged.Count > 0 ? merged : null;
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

        return DiscoverStepsFromUiDirectory(contentRootPath);
    }

    private static IReadOnlyList<string> DiscoverStepsFromUiDirectory(string? contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(contentRootPath))
        {
            return [];
        }

        var uiDir = Path.Combine(contentRootPath, "ui");
        if (!Directory.Exists(uiDir))
        {
            return [];
        }

        try
        {
            var settingsFiles = Directory
                .EnumerateFiles(uiDir, "Settings.json", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal);

            return settingsFiles
                .SelectMany(ReadStepsFromSettingsFile)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<string> ReadStepsFromSettingsFile(string settingsFile)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(settingsFile));
        if (
            !doc.RootElement.TryGetProperty("pages", out var pages)
            || !pages.TryGetProperty("order", out var order)
            || order.ValueKind != JsonValueKind.Array
        )
        {
            return [];
        }

        return order
            .EnumerateArray()
            .Select(item => item.GetString())
            .Where(step => !string.IsNullOrWhiteSpace(step))
            .Select(step => step!)
            .ToList();
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

    [GeneratedRegex("^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$", RegexOptions.IgnoreCase)]
    private static partial Regex UuidPattern();

    [GeneratedRegex("^/[a-z0-9-]+/[a-z0-9-]+$", RegexOptions.IgnoreCase)]
    private static partial Regex AppPathPattern();
}
