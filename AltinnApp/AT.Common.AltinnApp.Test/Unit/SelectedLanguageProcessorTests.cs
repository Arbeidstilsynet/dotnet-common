using System.Security.Claims;
using Altinn.App.Core.Internal.Language;
using Altinn.App.Core.Internal.Profile;
using Altinn.Platform.Profile.Models;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Arbeidstilsynet.Common.AltinnApp.Test.Unit.TestFixtures;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Xunit;
using ApplicationLanguageModel = Altinn.App.Core.Models.ApplicationLanguage;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class SelectedLanguageProcessorTests
{
    private const int UserId = 12345;
    private readonly IHttpContextAccessor _httpContextAccessor =
        Substitute.For<IHttpContextAccessor>();
    private readonly IProfileClient _profileClient = Substitute.For<IProfileClient>();
    private readonly ILanguageObserver _languageObserver = Substitute.For<ILanguageObserver>();
    private readonly IApplicationLanguage _applicationLanguage =
        Substitute.For<IApplicationLanguage>();
    private readonly SelectedLanguageProcessor _sut;
    private readonly Instance _instance;
    private readonly Guid _dataId = Guid.NewGuid();

    public SelectedLanguageProcessorTests()
    {
        _sut = new SelectedLanguageProcessor(
            _httpContextAccessor,
            _profileClient,
            _languageObserver,
            _applicationLanguage
        );
        _instance = AltinnData.CreateTestInstance();
        SetupHttpContextWithUserId(UserId);
        SetupAvailableLanguages("nb", "nn", "en");
    }

    [Fact]
    public async Task ProcessDataRead_WhenLanguageIsProvided_ShouldNotifyWithProvidedLanguage()
    {
        // Act
        await _sut.ProcessDataRead(_instance, _dataId, new object(), "en");

        // Assert
        await _languageObserver.Received(1).NotifyCurrentLanguage(Arg.Any<object>(), "en");
    }

    [Fact]
    public async Task ProcessDataRead_WhenLanguageIsNull_AndProfileHasLanguage_ShouldNotifyWithProfileLanguage()
    {
        // Arrange
        _profileClient
            .GetUserProfile(UserId)
            .Returns(
                new UserProfile
                {
                    ProfileSettingPreference = new ProfileSettingPreference { Language = "nb" },
                }
            );

        // Act
        await _sut.ProcessDataRead(_instance, _dataId, new object(), null);

        // Assert
        await _languageObserver.Received(1).NotifyCurrentLanguage(Arg.Any<object>(), "nb");
    }

    [Fact]
    public async Task ProcessDataRead_WhenLanguageIsEmpty_AndProfileHasLanguage_ShouldNotifyWithProfileLanguage()
    {
        // Arrange
        _profileClient
            .GetUserProfile(UserId)
            .Returns(
                new UserProfile
                {
                    ProfileSettingPreference = new ProfileSettingPreference { Language = "nn" },
                }
            );

        // Act
        await _sut.ProcessDataRead(_instance, _dataId, new object(), "");

        // Assert
        await _languageObserver.Received(1).NotifyCurrentLanguage(Arg.Any<object>(), "nn");
    }

    [Fact]
    public async Task ProcessDataRead_WhenLanguageIsNull_AndNoProfile_ShouldNotNotify()
    {
        // Arrange
        _profileClient.GetUserProfile(UserId).Returns((UserProfile?)null);

        // Act
        await _sut.ProcessDataRead(_instance, _dataId, new object(), null);

        // Assert
        await _languageObserver
            .DidNotReceiveWithAnyArgs()
            .NotifyCurrentLanguage(default!, default!);
    }

    [Fact]
    public async Task ProcessDataRead_WhenLanguageIsNull_AndNoHttpContext_ShouldNotNotify()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        await _sut.ProcessDataRead(_instance, _dataId, new object(), null);

        // Assert
        await _languageObserver
            .DidNotReceiveWithAnyArgs()
            .NotifyCurrentLanguage(default!, default!);
    }

    [Fact]
    public async Task ProcessDataRead_WhenLanguageIsNull_AndProfileLanguageIsEmpty_ShouldNotNotify()
    {
        // Arrange
        _profileClient
            .GetUserProfile(UserId)
            .Returns(
                new UserProfile
                {
                    ProfileSettingPreference = new ProfileSettingPreference { Language = "" },
                }
            );

        // Act
        await _sut.ProcessDataRead(_instance, _dataId, new object(), null);

        // Assert
        await _languageObserver
            .DidNotReceiveWithAnyArgs()
            .NotifyCurrentLanguage(default!, default!);
    }

    [Fact]
    public async Task ProcessDataRead_WhenLanguageIsNotSupported_ShouldNotNotify()
    {
        // Arrange
        SetupAvailableLanguages("nb", "nn");

        // Act
        await _sut.ProcessDataRead(_instance, _dataId, new object(), "fr");

        // Assert
        await _languageObserver
            .DidNotReceiveWithAnyArgs()
            .NotifyCurrentLanguage(default!, default!);
    }

    [Fact]
    public async Task ProcessDataRead_WhenProfileLanguageIsNotSupported_ShouldNotNotify()
    {
        // Arrange
        SetupAvailableLanguages("nb", "nn");
        _profileClient
            .GetUserProfile(UserId)
            .Returns(
                new UserProfile
                {
                    ProfileSettingPreference = new ProfileSettingPreference { Language = "de" },
                }
            );

        // Act
        await _sut.ProcessDataRead(_instance, _dataId, new object(), null);

        // Assert
        await _languageObserver
            .DidNotReceiveWithAnyArgs()
            .NotifyCurrentLanguage(default!, default!);
    }

    [Fact]
    public async Task ProcessDataWrite_ShouldCompleteWithoutCallingObserver()
    {
        // Act
        await _sut.ProcessDataWrite(_instance, _dataId, new object(), null, "en");

        // Assert
        await _languageObserver
            .DidNotReceiveWithAnyArgs()
            .NotifyCurrentLanguage(default!, default!);
    }

    private void SetupAvailableLanguages(params string[] languages)
    {
        var models = languages.Select(l => new ApplicationLanguageModel { Language = l }).ToList();
        _applicationLanguage.GetApplicationLanguages().Returns(models);
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
