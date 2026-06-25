using Microsoft.Kiota.Bundle;

namespace Arbeidstilsynet.Common.Saksarkiv.Implementation;

internal class SaksarkivRequestAdapter(
    SaksarkivAuthAdapter authenticationProvider,
    IHttpClientFactory httpClientFactory
)
    : DefaultRequestAdapter(
        authenticationProvider,
        httpClient: httpClientFactory.CreateClient(
            DependencyInjection.DependencyInjectionExtensions.SaksarkivHttpClientName
        )
    ) { }
