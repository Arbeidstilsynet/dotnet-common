using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

public static class HostEnvironmentExtensions
{
    public static string GetAltinnPlattformUrl(this IWebHostEnvironment webHostEnvironment)
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

    public static string GetAltinnAppBaseUrl(
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
