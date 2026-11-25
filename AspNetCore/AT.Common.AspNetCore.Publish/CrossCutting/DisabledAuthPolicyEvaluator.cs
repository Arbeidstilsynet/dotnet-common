using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// Skips authentication for policies with explicit authentication schemes, since no schemes are registered when
/// <see cref="AuthConfiguration.DisableAuth"/> is true. <see cref="HttpContext.User"/> is left unchanged.
/// </summary>
internal sealed class DisabledAuthPolicyEvaluator(IAuthorizationService authorization)
    : PolicyEvaluator(authorization)
{
    public override Task<AuthenticateResult> AuthenticateAsync(
        AuthorizationPolicy policy,
        HttpContext context
    )
    {
        if (policy.AuthenticationSchemes.Count > 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        return base.AuthenticateAsync(policy, context);
    }
}
