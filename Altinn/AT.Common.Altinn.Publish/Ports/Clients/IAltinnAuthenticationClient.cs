using System.IdentityModel.Tokens.Jwt;
using Altinn.App.Core.Infrastructure.Clients.Events;
using Arbeidstilsynet.Common.Altinn.Model.Api;
using SubscriptionRequest = Altinn.App.Core.Infrastructure.Clients.Events.SubscriptionRequest;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

/// <summary>
/// Client for Authentication
/// </summary>
public interface IAltinnAuthenticationClient
{
    /// <summary>
    /// Exchanges a token from an external authentication provider for an Altinn token.
    /// </summary>
    /// <param name="tokenProvider"></param>
    /// <param name="tokenProviderToken"></param>
    /// <returns></returns>
    Task<string> ExchangeToken(
        AuthenticationTokenProvider tokenProvider,
        string tokenProviderToken
    );
}
