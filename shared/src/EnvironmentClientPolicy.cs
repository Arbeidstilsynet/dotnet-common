using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Shared.DependencyInjection;

internal static class EnvironmentClientPolicy
{
    public static T ResolveTarget<T>(
        IHostEnvironment host,
        T? requested,
        T production,
        T staging,
        T development
    )
        where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(host);
        if (requested is { } value && !Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(requested),
                value,
                "Unknown client environment."
            );
        }

        if (host.IsProduction())
        {
            if (requested is { } target && !target.Equals(production))
            {
                throw new InvalidOperationException(
                    "Production applications must target the production API."
                );
            }

            return production;
        }

        if (host.IsStaging())
        {
            return requested ?? staging;
        }

        if (host.IsDevelopment())
        {
            return requested ?? development;
        }

        return requested
            ?? throw new InvalidOperationException(
                $"Set the client configuration's Environment explicitly for host environment '{host.EnvironmentName}'."
            );
    }

    public static Uri ResolveUrl(IHostEnvironment host, string? configured, string defaultUrl)
    {
        if (
            !Uri.TryCreate(configured ?? defaultUrl, UriKind.Absolute, out var url)
            || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
        )
        {
            throw new ArgumentException(
                "Client base URLs must use HTTP or HTTPS.",
                nameof(configured)
            );
        }

        if (
            !string.IsNullOrEmpty(url.Query)
            || !string.IsNullOrEmpty(url.Fragment)
            || !string.IsNullOrEmpty(url.UserInfo)
        )
        {
            throw new ArgumentException(
                "Client base URLs must not contain credentials, a query or fragment.",
                nameof(configured)
            );
        }

        if (host.IsProduction() && !SameUrl(url, new Uri(defaultUrl)))
        {
            throw new InvalidOperationException(
                "Overriding client base URLs is not permitted in Production."
            );
        }

        return url;
    }

    public static void WarnOnStagingOverrides(
        IServiceCollection services,
        IHostEnvironment host,
        string client,
        params (Uri Actual, string Default)[] urls
    )
    {
        var overrides = urls.Where(pair => !SameUrl(pair.Actual, new Uri(pair.Default)))
            .Select(pair => pair.Actual.GetLeftPart(UriPartial.Path))
            .ToArray();
        if (host.IsStaging() && overrides.Length > 0)
        {
            services.AddSingleton<IHostedService>(provider => new OverrideWarningService(
                provider.GetRequiredService<ILogger<OverrideWarningService>>(),
                client,
                overrides
            ));
        }
    }

    private static bool SameUrl(Uri left, Uri right) =>
        left.AbsoluteUri.TrimEnd('/') == right.AbsoluteUri.TrimEnd('/');

    private sealed class OverrideWarningService(
        ILogger<OverrideWarningService> logger,
        string client,
        string[] urls
    ) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogWarning(
                "{Client} base URLs have been overridden in Staging: {Urls}",
                client,
                string.Join(", ", urls)
            );
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
