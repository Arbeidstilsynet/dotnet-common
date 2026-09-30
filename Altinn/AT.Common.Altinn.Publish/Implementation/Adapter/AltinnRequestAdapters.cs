using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation.Authentication;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;
using static Arbeidstilsynet.Common.Altinn.DependencyInjection.DependencyInjectionExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

/// <summary>
/// Builds the authentication provider for a named client, carrying that client's scopes.
/// </summary>
internal static class AltinnRequestAdapterFactory
{
    public static AltinnAuthenticationProvider AuthenticationFor(
        IAltinnTokenProvider tokenProvider,
        IOptionsMonitor<AltinnClientOptions> options,
        string clientName
    ) => new(tokenProvider, options.Get(clientName).Scopes ?? []);
}

/// <summary>
/// Request adapters binding each generated client to its named <see cref="HttpClient"/>, so that the
/// configured base address and resilience handler apply.
/// </summary>
/// <remarks>
/// Each adapter authenticates with the scopes registered for its own API, so a client presents only
/// the scopes it needs. The base URL is assigned explicitly during registration: the generated
/// clients otherwise fall back to the server declared in their specification, which is TT02 -- a
/// production application would silently talk to the test environment.
/// </remarks>
internal class StorageRequestAdapter(
    IHttpClientFactory httpClientFactory,
    IAltinnTokenProvider tokenProvider,
    IOptionsMonitor<AltinnClientOptions> options
)
    : DefaultRequestAdapter(
        AltinnRequestAdapterFactory.AuthenticationFor(
            tokenProvider,
            options,
            AltinnClients.Storage
        ),
        httpClient: httpClientFactory.CreateClient(AltinnStorageApiClientKey)
    ) { }

internal class EventsRequestAdapter(
    IHttpClientFactory httpClientFactory,
    IAltinnTokenProvider tokenProvider,
    IOptionsMonitor<AltinnClientOptions> options
)
    : DefaultRequestAdapter(
        AltinnRequestAdapterFactory.AuthenticationFor(tokenProvider, options, AltinnClients.Events),
        httpClient: httpClientFactory.CreateClient(AltinnEventsApiClientKey)
    ) { }

internal class AppsRequestAdapter(
    IHttpClientFactory httpClientFactory,
    IAltinnTokenProvider tokenProvider,
    IOptionsMonitor<AltinnClientOptions> options
)
    : DefaultRequestAdapter(
        AltinnRequestAdapterFactory.AuthenticationFor(tokenProvider, options, AltinnClients.Apps),
        httpClient: httpClientFactory.CreateClient(AltinnAppsApiClientKey)
    ) { }

internal class CorrespondenceRequestAdapter(
    IHttpClientFactory httpClientFactory,
    IAltinnTokenProvider tokenProvider,
    IOptionsMonitor<AltinnClientOptions> options
)
    : DefaultRequestAdapter(
        AltinnRequestAdapterFactory.AuthenticationFor(
            tokenProvider,
            options,
            AltinnClients.Correspondence
        ),
        httpClient: httpClientFactory.CreateClient(AltinnCorrespondenceApiClientKey)
    ) { }

internal class DialogportenRequestAdapter(
    IHttpClientFactory httpClientFactory,
    IAltinnTokenProvider tokenProvider,
    IOptionsMonitor<AltinnClientOptions> options
)
    : DefaultRequestAdapter(
        AltinnRequestAdapterFactory.AuthenticationFor(
            tokenProvider,
            options,
            AltinnClients.Dialogporten
        ),
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
