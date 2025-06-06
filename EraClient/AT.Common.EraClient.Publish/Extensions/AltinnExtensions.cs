using Arbeidstilsynet.Common.EraClient.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.EraClient.Extensions;

public static class AltinnExtensions
{
    public static EraEnvironment GetRespectiveEraEnvironment(
        this IHostEnvironment hostEnvironment,
        HttpContext? httpContext = null
    )
    {
        if (hostEnvironment.IsProduction())
        {
            return EraEnvironment.Prod;
        }
        else
        {
            if (httpContext != null && hostEnvironment.IsFeatureEnabled("valid", httpContext))
            {
                return EraEnvironment.Valid;
            }
            return EraEnvironment.Verifi;
        }
    }

    public static string MapToString(this EraEnvironment eraEnvironment) =>
        eraEnvironment.ToString().ToLower();

    public static bool IsFeatureEnabled(
        this IHostEnvironment env,
        string name,
        HttpContext httpContext
    )
    {
        bool isNotProduction = !env.IsProduction();

        var testFlagString = httpContext.Request.Cookies["TEST_FLAGG"];
        bool containsFeatureFlag =
            testFlagString != null && testFlagString.Split('&').Any(a => a == name);

        return isNotProduction && containsFeatureFlag;
    }
}
