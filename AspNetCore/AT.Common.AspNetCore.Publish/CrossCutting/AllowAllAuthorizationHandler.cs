using Microsoft.AspNetCore.Authorization;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// Succeeds every authorization requirement. Only registered when <see cref="AuthConfiguration.DisableAuth"/> is true,
/// so that role-based and named policies behave like the default policy in local development and tests.
/// </summary>
internal sealed class AllowAllAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
