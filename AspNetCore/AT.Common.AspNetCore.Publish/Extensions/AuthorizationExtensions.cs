using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds authorization policies based on role to group ID mappings.<br />
    /// This can be used to handle authorization when JWT claims have groups but not roles.<br />
    /// Usage:<br/>
    /// [HttpGet("foo")]<br />
    /// [Authorize(Policy = CustomRoleName)]
    /// </summary>
    /// <param name="services">The service collection to add the policies to.</param>
    /// <param name="roleGroupMappings">A dictionary mapping role names to arrays of allowed group IDs.</param>
    /// <param name="groupClaimType">The claim type used for group IDs. Defaults to "groups".</param>
    public static void AddGroupRoleMappings(
        this IServiceCollection services,
        IReadOnlyDictionary<string, IEnumerable<string>> roleGroupMappings,
        string groupClaimType = "groups"
    )
    {
        services.AddAuthorization(options =>
        {
            foreach (var mapping in roleGroupMappings)
            {
                var allowed = (mapping.Value ?? Array.Empty<string>()).ToHashSet(
                    StringComparer.OrdinalIgnoreCase
                );

                options.AddPolicy(
                    mapping.Key,
                    policy =>
                        policy
                            .RequireAuthenticatedUser()
                            .RequireAssertion(context =>
                                context.User.HasAnyAllowedGroup(allowed, groupClaimType)
                            )
                );
            }
        });
    }

    private static bool HasAnyAllowedGroup(
        this ClaimsPrincipal user,
        HashSet<string> allowedGroupIds,
        string groupClaimType
    )
    {
        if (allowedGroupIds is null || allowedGroupIds.Count == 0)
        {
            return false;
        }

        return user.FindAll(groupClaimType).Select(c => c.Value).Any(allowedGroupIds.Contains);
    }
}
