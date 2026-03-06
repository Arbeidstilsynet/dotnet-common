using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Helper methods for creating default Altinn configuration based on the current host environment.
/// </summary>
public static class HostEnvironmentExtensions
{
    private const string AltinnAuthenticationApiSuffix = "authentication/api/v1/";
    private const string AltinnEventApiSuffix = "events/api/v1/";

    private const string AltinnStorageApiSuffix = "storage/api/v1/";

    /// <summary>
    /// Creates a default <see cref="AltinnConfiguration"/> for the current web host environment.
    /// <br/>
    /// - Development: Uses local.altinn.cloud.
    /// <br/>
    /// - Staging: Uses TT02.
    /// <br/>
    /// - Production: Uses production Altinn URLs.
    /// </summary>
    /// <param name="webHostEnvironment">The web host environment.</param>
    /// <param name="orgId">Used to build the <see cref="AltinnConfiguration.AppBaseUrl"/>. Defaults to "dat" (Arbeidstilsynet)</param>
    /// <returns>A default Altinn API configuration for the environment.</returns>
    public static AltinnConfiguration CreateDefaultAltinnConfiguration(
        this IWebHostEnvironment webHostEnvironment,
        string orgId = "dat"
    )
    {
        return new AltinnConfiguration()
        {
            OrgId = orgId,
            AuthenticationUrl = new Uri(
                new Uri(webHostEnvironment.GetAltinnPlattformUrl()),
                AltinnAuthenticationApiSuffix
            ),
            EventUrl = new Uri(
                new Uri(webHostEnvironment.GetAltinnPlattformUrl()),
                AltinnEventApiSuffix
            ),
            StorageUrl = new Uri(
                new Uri(webHostEnvironment.GetAltinnPlattformUrl()),
                AltinnStorageApiSuffix
            ),
            AppBaseUrl = new Uri(webHostEnvironment.GetAltinnAppBaseUrl(orgId)),
        };
    }

    /// <summary>
    /// Returns the base URL for Maskinporten for the current environment.
    /// </summary>
    /// <param name="webHostEnvironment">The web host environment.</param>
    /// <returns>Maskinporten base URL.</returns>
    public static string GetMaskinportenUrl(this IWebHostEnvironment webHostEnvironment)
    {
        if (webHostEnvironment.IsProduction())
        {
            return "https://maskinporten.no/";
        }
        else
        {
            return "https://test.maskinporten.no/";
        }
    }

    private static string GetAltinnPlattformUrl(this IWebHostEnvironment webHostEnvironment)
    {
        if (webHostEnvironment.IsDevelopment())
        {
            return "http://local.altinn.cloud:5101/";
        }
        else if (webHostEnvironment.IsProduction())
        {
            return "https://platform.altinn.no/";
        }
        else
        {
            return "https://platform.tt02.altinn.no/";
        }
    }

    private static string GetAltinnAppBaseUrl(
        this IWebHostEnvironment webHostEnvironment,
        string orgId
    )
    {
        if (webHostEnvironment.IsDevelopment())
        {
            return "http://local.altinn.cloud:5005/";
        }
        else if (webHostEnvironment.IsProduction())
        {
            return $"https://{orgId}.apps.altinn.no/";
        }
        else
        {
            return $"https://{orgId}.apps.tt02.altinn.no/";
        }
    }
}
