using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

internal class EnhetsregisteretRequestAdapter : DefaultRequestAdapter
{
    public EnhetsregisteretRequestAdapter(IHttpClientFactory httpClientFactory)
        : base(
            new AnonymousAuthenticationProvider(),
            httpClient: httpClientFactory.CreateClient(DependencyInjectionExtensions.Clientkey)
        )
    {
        // The named HttpClient's BaseAddress carries a trailing slash (Uri normalizes an empty
        // path to "/"), which Kiota copies into BaseUrl. Combined with URL templates like
        // "{+baseurl}/enhetsregisteret", that produces double slashes in request URLs. Trim it so
        // requests match the generated client's default base URL.
        if (!string.IsNullOrEmpty(BaseUrl))
        {
            BaseUrl = BaseUrl.TrimEnd('/');
        }
    }
}
