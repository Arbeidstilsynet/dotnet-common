using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal class EiendomRequestAdapter(IHttpClientFactory httpClientFactory)
    : DefaultRequestAdapter(
        new AnonymousAuthenticationProvider(),
        httpClient: httpClientFactory.CreateClient(
            DependencyInjection.DependencyInjectionExtensions.EiendomHttpClientName
        )
    ) { }
