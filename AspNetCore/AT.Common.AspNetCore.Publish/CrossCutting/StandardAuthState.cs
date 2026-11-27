using System.Security.Claims;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// Holds the <see cref="AuthConfiguration"/> passed to AddStandardAuth, so it can be used at request time.
/// </summary>
internal sealed class StandardAuthState(AuthConfiguration authConfiguration)
{
    public string[] GetRoles(ClaimsPrincipal user)
    {
        var roles = authConfiguration.Roles.Keys;

        return authConfiguration.DisableAuth
            ? roles.ToArray()
            : roles.Where(user.IsInRole).ToArray();
    }
}
