using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

public static class HostEnvironmentExtensions
{
    private const string AltinnAuthenticationApiSuffix = "authentication/api/v1/";
    private const string AltinnEventApiSuffix = "events/api/v1/";

    private const string AltinnStorageApiSuffix = "storage/api/v1/";

    /// <summary>
    /// Creates a default <see cref="AltinnApiConfiguration"/> for the current web host environment.
    /// <br/>
    /// - Development: Uses local.altinn.cloud.
    /// <br/>
    /// - Staging: Uses TT02.
    /// <br/>
    /// - Production: Uses production Altinn URLs.
    /// </summary>
    /// <param name="webHostEnvironment">The web host environment.</param>
    /// <returns>A default Altinn API configuration for the environment.</returns>
    public static AltinnApiConfiguration CreateDefaultAltinnApiConfiguration(
        this IWebHostEnvironment webHostEnvironment
    )
    {
        return new AltinnApiConfiguration()
        {
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
            AppBaseUrl = new Uri(
                webHostEnvironment.GetAltinnAppBaseUrl(
                    DependencyInjectionExtensions.AltinnOrgIdentifier
                )
            ),
        };
    }

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
        string org
    )
    {
        if (webHostEnvironment.IsDevelopment())
        {
            return "http://local.altinn.cloud:5005/";
        }
        else if (webHostEnvironment.IsProduction())
        {
            return $"https://{org}.apps.altinn.no/{org}/";
            ;
        }
        else
        {
            return $"https://{org}.apps.tt02.altinn.no/{org}/";
        }
    }
}
