using Arbeidstilsynet.Common.Enhetsregisteret.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

internal class EnhetsregisteretRequestAdapter(IHttpClientFactory httpClientFactory)
    : DefaultRequestAdapter(
        new AnonymousAuthenticationProvider(),
        httpClient: httpClientFactory.CreateClient(DependencyInjectionExtensions.Clientkey)
    ) { }
