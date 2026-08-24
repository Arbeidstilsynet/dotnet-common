using Arbeidstilsynet.Common.Altinn.Implementation.Authentication;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

/// <summary>
/// Request adapters binding each generated client to its named <see cref="HttpClient"/>, so that the
/// configured base address and resilience handler apply.
/// </summary>
/// <remarks>
/// The base URL is assigned explicitly during registration. The generated clients otherwise fall
/// back to the server declared in their specification, which is TT02 -- a production application
/// would silently talk to the test environment.
/// </remarks>
internal class StorageRequestAdapter(
    IHttpClientFactory httpClientFactory,
    AltinnAuthenticationProvider authenticationProvider
)
    : DefaultRequestAdapter(
        authenticationProvider,
        httpClient: httpClientFactory.CreateClient(AltinnStorageApiClientKey)
    ) { }

internal class EventsRequestAdapter(
    IHttpClientFactory httpClientFactory,
    AltinnAuthenticationProvider authenticationProvider
)
    : DefaultRequestAdapter(
        authenticationProvider,
        httpClient: httpClientFactory.CreateClient(AltinnEventsApiClientKey)
    ) { }

internal class AppsRequestAdapter(
    IHttpClientFactory httpClientFactory,
    AltinnAuthenticationProvider authenticationProvider
)
    : DefaultRequestAdapter(
        authenticationProvider,
        httpClient: httpClientFactory.CreateClient(AltinnAppsApiClientKey)
    ) { }

internal class CorrespondenceRequestAdapter(
    IHttpClientFactory httpClientFactory,
    AltinnAuthenticationProvider authenticationProvider
)
    : DefaultRequestAdapter(
        authenticationProvider,
        httpClient: httpClientFactory.CreateClient(AltinnCorrespondenceApiClientKey)
    ) { }

internal class DialogportenRequestAdapter(
    IHttpClientFactory httpClientFactory,
    AltinnAuthenticationProvider authenticationProvider
)
    : DefaultRequestAdapter(
        authenticationProvider,
        httpClient: httpClientFactory.CreateClient(DialogportenApiClientKey)
    ) { }

/// <summary>
/// The authentication API is what issues Altinn tokens, so it authenticates anonymously and is
/// given the Maskinporten bearer token per request.
/// </summary>
internal class AuthenticationRequestAdapter(IHttpClientFactory httpClientFactory)
    : DefaultRequestAdapter(
        new AnonymousAuthenticationProvider(),
        httpClient: httpClientFactory.CreateClient(AltinnAuthenticationApiClientKey)
    ) { }
