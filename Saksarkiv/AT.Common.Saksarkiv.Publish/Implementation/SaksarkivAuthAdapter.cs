using Arbeidstilsynet.Common.Saksarkiv.DependencyInjection.Configuration;
using Arbeidstilsynet.Common.Saksarkiv.Ports;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Arbeidstilsynet.Common.Saksarkiv.Implementation;

internal class SaksarkivAuthAdapter(
    ISaksarkivTokenProvider tokenProvider,
    SaksarkivConfiguration configuration
) : IAuthenticationProvider
{
    private readonly string _scope = configuration.Scope;

    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default
    )
    {
        var accessToken = await tokenProvider.GetAccessToken(_scope, cancellationToken);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
    }
}
