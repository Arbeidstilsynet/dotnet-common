using Altinn.App.Core.Features;
using Altinn.App.Core.Internal.Language;
using Altinn.App.Core.Internal.Profile;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal class SelectedLanguageProcessor : IDataProcessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProfileClient _profileClient;
    private readonly ILanguageObserver _languageObserver;
    private readonly IApplicationLanguage _applicationLanguage;

    public SelectedLanguageProcessor(
        IHttpContextAccessor httpContextAccessor,
        IProfileClient profileClient,
        ILanguageObserver languageObserver,
        IApplicationLanguage applicationLanguage
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _profileClient = profileClient;
        _languageObserver = languageObserver;
        _applicationLanguage = applicationLanguage;
    }

    public async Task ProcessDataRead(
        Instance instance,
        Guid? dataId,
        object data,
        string? language
    )
    {
        var selectedLanguage = language;

        if (selectedLanguage is not { Length: > 0 })
        {
            if (await _profileClient.GetUserProfile(_httpContextAccessor) is { } userProfile)
            {
                selectedLanguage = userProfile.ProfileSettingPreference.Language;
            }
        }

        var availableLanguages = (await _applicationLanguage.GetApplicationLanguages()).Select(l =>
            l.Language
        );

        if (selectedLanguage is { Length: > 0 } && availableLanguages.Contains(selectedLanguage))
        {
            await _languageObserver.NotifyCurrentLanguage(data, selectedLanguage);
        }
    }

    public Task ProcessDataWrite(
        Instance instance,
        Guid? dataId,
        object data,
        object? previousData,
        string? language
    )
    {
        return Task.CompletedTask;
    }
}
