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
    Task<string> ExchangeToken(
        AuthenticationTokenProvider tokenProvider,
        string tokenProviderToken
    );
}
