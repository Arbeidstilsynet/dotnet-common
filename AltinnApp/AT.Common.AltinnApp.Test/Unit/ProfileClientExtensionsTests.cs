using System.Security.Claims;
using Altinn.App.Core.Internal.Profile;
using Altinn.Platform.Profile.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class ProfileClientExtensionsTests
{
    private const int UserId = 42;
    private readonly IProfileClient _profileClient = Substitute.For<IProfileClient>();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

    [Fact]
    public async Task GetUserProfile_WhenUserIsLoggedIn_ReturnsProfile()
    {
        // Arrange
        var expected = new UserProfile { UserId = UserId };
        SetupHttpContextWithUserId(UserId);
        _profileClient.GetUserProfile(UserId).Returns(expected);

        // Act
        var result = await _profileClient.GetUserProfile(_httpContextAccessor);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task GetUserProfile_WhenHttpContextIsNull_ReturnsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = await _profileClient.GetUserProfile(_httpContextAccessor);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetUserProfile_WhenUserHasNoClaims_ReturnsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = await _profileClient.GetUserProfile(_httpContextAccessor);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetUserProfile_WhenProfileClientReturnsNull_ReturnsNull()
    {
        // Arrange
        SetupHttpContextWithUserId(UserId);
        _profileClient.GetUserProfile(UserId).Returns((UserProfile?)null);

        // Act
        var result = await _profileClient.GetUserProfile(_httpContextAccessor);

        // Assert
        result.ShouldBeNull();
    }

    private void SetupHttpContextWithUserId(int userId)
    {
        var claims = new[] { new Claim("urn:altinn:userid", userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);
    }
}
