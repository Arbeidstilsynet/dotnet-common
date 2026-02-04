using System.Security.Claims;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

public class AuthorizationExtensionsTests
{
    private const string PolicyName = "reader";
    private const string DefaultGroupClaimType = "groups";

    [Fact]
    public async Task AddGroupRoleMappings_WhenUserAuthenticatedAndInAllowedGroup_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGroupRoleMappings(
            new Dictionary<string, IEnumerable<string>> { [PolicyName] = ["abc"] },
            groupClaimType: DefaultGroupClaimType
        );

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = CreateUser(isAuthenticated: true, (DefaultGroupClaimType, "ABC"));

        // Act
        var result = await authorizationService.AuthorizeAsync(
            user,
            resource: null,
            policyName: PolicyName
        );

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task AddGroupRoleMappings_WhenUserAuthenticatedButNotInAllowedGroup_Fails()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGroupRoleMappings(
            new Dictionary<string, IEnumerable<string>> { [PolicyName] = ["foo", "bar"] },
            groupClaimType: DefaultGroupClaimType
        );

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = CreateUser(isAuthenticated: true, (DefaultGroupClaimType, "def"));

        // Act
        var result = await authorizationService.AuthorizeAsync(
            user,
            resource: null,
            policyName: PolicyName
        );

        // Assert
        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task AddGroupRoleMappings_WhenUserNotAuthenticated_EvenIfInAllowedGroup_Fails()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGroupRoleMappings(
            new Dictionary<string, IEnumerable<string>> { [PolicyName] = ["abc"] },
            groupClaimType: DefaultGroupClaimType
        );

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = CreateUser(isAuthenticated: false, (DefaultGroupClaimType, "abc"));

        // Act
        var result = await authorizationService.AuthorizeAsync(
            user,
            resource: null,
            policyName: PolicyName
        );

        // Assert
        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task AddGroupRoleMappings_WhenAllowedGroupListIsEmpty_Fails()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGroupRoleMappings(
            new Dictionary<string, IEnumerable<string>> { [PolicyName] = Array.Empty<string>() },
            groupClaimType: DefaultGroupClaimType
        );

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var user = CreateUser(isAuthenticated: true, (DefaultGroupClaimType, "abc"));

        // Act
        var result = await authorizationService.AuthorizeAsync(
            user,
            resource: null,
            policyName: PolicyName
        );

        // Assert
        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task AddGroupRoleMappings_WhenCustomGroupClaimTypeIsUsed_RespectsClaimType()
    {
        // Arrange
        const string customClaimType = "mygroups";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGroupRoleMappings(
            new Dictionary<string, IEnumerable<string>> { [PolicyName] = ["abc"] },
            groupClaimType: customClaimType
        );

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        // Has matching group id, but on the default claim type, not the configured one.
        var user = CreateUser(isAuthenticated: true, (DefaultGroupClaimType, "abc"));

        // Act
        var result = await authorizationService.AuthorizeAsync(
            user,
            resource: null,
            policyName: PolicyName
        );

        // Assert
        result.Succeeded.ShouldBeFalse();

        // Act (with matching claim type)
        var userWithCustomClaim = CreateUser(isAuthenticated: true, (customClaimType, "abc"));
        var result2 = await authorizationService.AuthorizeAsync(
            userWithCustomClaim,
            resource: null,
            policyName: PolicyName
        );

        // Assert
        result2.Succeeded.ShouldBeTrue();
    }

    private static ClaimsPrincipal CreateUser(
        bool isAuthenticated,
        params (string Type, string Value)[] claims
    )
    {
        var identityClaims = claims.Select(c => new Claim(c.Type, c.Value));
        var identity = isAuthenticated
            ? new ClaimsIdentity(identityClaims, authenticationType: "TestAuth")
            : new ClaimsIdentity(identityClaims);

        return new ClaimsPrincipal(identity);
    }
}
