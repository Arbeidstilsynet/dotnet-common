using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;

/// <summary>
/// Maps Entra ID <c>groups</c> claims to <see cref="ClaimTypes.Role"/> claims based on <see cref="AuthConfiguration.Roles"/>.
/// </summary>
internal sealed class GroupRoleClaimsTransformation : IClaimsTransformation
{
    internal const string GroupsClaimType = "groups";

    // JwtBearer maps "groups" to this claim type when MapInboundClaims is true (the default).
    internal const string MappedGroupsClaimType =
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/groups";

    internal const string AuthenticationType = "GroupRoleMapping";

    private readonly IReadOnlyDictionary<string, IReadOnlyList<Guid>> _roles;

    public GroupRoleClaimsTransformation(AuthConfiguration authConfiguration)
    {
        _roles = authConfiguration.Roles;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var groups = principal
            .FindAll(c => c.Type is GroupsClaimType or MappedGroupsClaimType)
            .Select(c => Guid.TryParse(c.Value, out var id) ? id : (Guid?)null)
            .OfType<Guid>()
            .ToHashSet();

        if (groups.Count == 0)
        {
            return Task.FromResult(principal);
        }

        var rolesToAdd = _roles
            .Where(role => !principal.IsInRole(role.Key) && role.Value.Any(groups.Contains))
            .Select(role => new Claim(ClaimTypes.Role, role.Key))
            .ToList();

        if (rolesToAdd.Count == 0)
        {
            return Task.FromResult(principal);
        }

        var transformed = principal.Clone();
        transformed.AddIdentity(
            new ClaimsIdentity(rolesToAdd, AuthenticationType, ClaimTypes.Name, ClaimTypes.Role)
        );

        return Task.FromResult(transformed);
    }
}
