using System.Net.Http.Json;
using System.Security.Claims;
using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

public class AuthRolesTests
{
    private static readonly Guid LeserGroup = Guid.NewGuid();
    private static readonly Guid AltLeserGroup = Guid.NewGuid();
    private static readonly Guid SensitivGroup = Guid.NewGuid();

    private static AuthConfiguration CreateConfig(bool disableAuth = false) =>
        new()
        {
            DisableAuth = disableAuth,
            TenantId = "fake-tenant",
            ClientId = "fake-client",
            Scope = "fake-scope",
            Roles = new Dictionary<string, IReadOnlyList<Guid>>
            {
                ["Leser"] = [LeserGroup, AltLeserGroup],
                ["Sensitiv"] = [SensitivGroup],
            },
        };

    private static ClaimsPrincipal CreateUser(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "Bearer"));

    [Theory]
    [InlineData("groups")]
    [InlineData("http://schemas.microsoft.com/ws/2008/06/identity/claims/groups")]
    public async Task TransformAsync_MapsGroupsToRoles(string groupsClaimType)
    {
        var sut = new GroupRoleClaimsTransformation(CreateConfig());
        var user = CreateUser(new Claim(groupsClaimType, AltLeserGroup.ToString()));

        var result = await sut.TransformAsync(user);

        result.IsInRole("Leser").ShouldBeTrue();
        result.IsInRole("Sensitiv").ShouldBeFalse();
    }

    [Fact]
    public async Task TransformAsync_CalledMultipleTimes_DoesNotDuplicateRoles()
    {
        var sut = new GroupRoleClaimsTransformation(CreateConfig());
        var user = CreateUser(
            new Claim("groups", LeserGroup.ToString()),
            new Claim("groups", AltLeserGroup.ToString())
        );

        var result = await sut.TransformAsync(await sut.TransformAsync(user));

        result.FindAll(ClaimTypes.Role).Select(c => c.Value).ShouldBe(["Leser"]);
    }

    [Fact]
    public async Task TransformAsync_RoleAlreadyPresent_IsNotAddedAgain()
    {
        var sut = new GroupRoleClaimsTransformation(CreateConfig());
        var user = CreateUser(
            new Claim(ClaimTypes.Role, "Sensitiv"),
            new Claim("groups", SensitivGroup.ToString())
        );

        var result = await sut.TransformAsync(user);

        result.ShouldBeSameAs(user);
        result.FindAll(ClaimTypes.Role).Count().ShouldBe(1);
    }

    [Fact]
    public async Task TransformAsync_InvalidGroupValues_AreIgnored()
    {
        var sut = new GroupRoleClaimsTransformation(CreateConfig());
        var user = CreateUser(
            new Claim("groups", "not-a-guid"),
            new Claim("groups", ""),
            new Claim("groups", SensitivGroup.ToString())
        );

        var result = await sut.TransformAsync(user);

        result.FindAll(ClaimTypes.Role).Select(c => c.Value).ShouldBe(["Sensitiv"]);
    }

    [Fact]
    public void AddStandardAuth_WithRoles_RegistersClaimsTransformation()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddStandardAuth(CreateConfig());

        services
            .BuildServiceProvider()
            .GetService<IClaimsTransformation>()
            .ShouldBeOfType<GroupRoleClaimsTransformation>();
    }

    [Fact]
    public void AddStandardAuth_WithoutRoles_DoesNotRegisterClaimsTransformation()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddStandardAuth(
            CreateConfig() with
            {
                Roles = new Dictionary<string, IReadOnlyList<Guid>>(),
            }
        );

        services.ShouldNotContain(s => s.ImplementationInstance is GroupRoleClaimsTransformation);
    }

    [Fact]
    public async Task AddStandardAuth_DisableAuth_RoleAndNamedPoliciesSucceed()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardAuth(CreateConfig(disableAuth: true));
        services
            .AddAuthorizationBuilder()
            .AddPolicy("Named", p => p.RequireClaim("groups", SensitivGroup.ToString()));
        var authorizationService = services
            .BuildServiceProvider()
            .GetRequiredService<IAuthorizationService>();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        var rolesResult = await authorizationService.AuthorizeAsync(
            anonymous,
            null,
            new AuthorizationPolicyBuilder().RequireRole("Leser").Build()
        );
        var namedResult = await authorizationService.AuthorizeAsync(anonymous, "Named");

        rolesResult.Succeeded.ShouldBeTrue();
        namedResult.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task AddStandardAuth_DisableAuth_PolicyWithExplicitlyFailingHandlerSucceeds()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardAuth(CreateConfig(disableAuth: true));
        services.AddSingleton<IAuthorizationHandler, FailingHandler>();
        services
            .AddAuthorizationBuilder()
            .AddPolicy("Failing", p => p.AddRequirements(new FailingRequirement()));
        var authorizationService = services
            .BuildServiceProvider()
            .GetRequiredService<IAuthorizationService>();

        var result = await authorizationService.AuthorizeAsync(
            new ClaimsPrincipal(new ClaimsIdentity()),
            "Failing"
        );

        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public async Task AddStandardAuth_DisableAuth_SchemeQualifiedPolicySucceeds()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddStandardAuth(CreateConfig(disableAuth: true));
        builder
            .Services.AddAuthorizationBuilder()
            .AddPolicy(
                "Bearer",
                new AuthorizationPolicyBuilder("Bearer").RequireRole("Leser").Build()
            );
        await using var app = builder.Build();
        app.UseAuthorization();
        app.MapGet("/protected", () => "ok").RequireAuthorization("Bearer");
        await app.StartAsync(TestContext.Current.CancellationToken);

        var response = await app.GetTestClient()
            .GetAsync("/protected", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    private sealed class FailingRequirement : IAuthorizationRequirement;

    private sealed class FailingHandler : AuthorizationHandler<FailingRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            FailingRequirement requirement
        )
        {
            context.Fail();
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task AddStandardAuth_AuthEnabled_RolePolicyRequiresRole()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardAuth(CreateConfig());
        var authorizationService = services
            .BuildServiceProvider()
            .GetRequiredService<IAuthorizationService>();

        var result = await authorizationService.AuthorizeAsync(
            CreateUser(),
            null,
            new AuthorizationPolicyBuilder().RequireRole("Leser").Build()
        );

        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public void AuthConfiguration_Roles_BindsFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["TenantId"] = "t",
                    ["ClientId"] = "c",
                    ["Scope"] = "s",
                    ["Roles:Leser:0"] = LeserGroup.ToString(),
                    ["Roles:Leser:1"] = AltLeserGroup.ToString(),
                    ["Roles:Sensitiv:0"] = SensitivGroup.ToString(),
                }
            )
            .Build();

        var result = configuration.Get<AuthConfiguration>()!;

        result.Roles["Leser"].ShouldBe([LeserGroup, AltLeserGroup]);
        result.Roles["Sensitiv"].ShouldBe([SensitivGroup]);
    }

    [Fact]
    public void AuthConfiguration_WithoutRoles_HasEmptyRoles()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["TenantId"] = "t",
                    ["ClientId"] = "c",
                    ["Scope"] = "s",
                }
            )
            .Build();

        configuration.Get<AuthConfiguration>()!.Roles.ShouldBeEmpty();
    }

    [Fact]
    public async Task MapUserRoles_DisableAuth_ReturnsAllConfiguredRoles()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddStandardAuth(CreateConfig(disableAuth: true));
        await using var app = builder.Build();
        app.UseAuthorization();
        app.MapUserRoles();
        await app.StartAsync(TestContext.Current.CancellationToken);

        var roles = await app.GetTestClient()
            .GetFromJsonAsync<string[]>("/roles", TestContext.Current.CancellationToken);

        roles.ShouldBe(["Leser", "Sensitiv"]);
    }

    [Fact]
    public void GetRoles_AuthEnabled_ReturnsOnlyRolesOfUser()
    {
        var sut = new StandardAuthState(CreateConfig());

        var roles = sut.GetRoles(CreateUser(new Claim(ClaimTypes.Role, "Sensitiv")));

        roles.ShouldBe(["Sensitiv"]);
    }
}
