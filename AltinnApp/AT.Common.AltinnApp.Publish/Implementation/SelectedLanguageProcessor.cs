using Altinn.App.Core.Features;
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

    public SelectedLanguageProcessor(
        IHttpContextAccessor httpContextAccessor, 
        IProfileClient profileClient,
        ILanguageObserver languageObserver)
    {
        _httpContextAccessor = httpContextAccessor;
        _profileClient = profileClient;
        _languageObserver = languageObserver;
    }
    
    public async Task ProcessDataRead(Instance instance, Guid? dataId, object data, string? language)
    {
        var språkvalg = language;

        if (språkvalg is not { Length: > 0 })
        {
            if (await _profileClient.GetUserProfile(_httpContextAccessor) is { } innloggetBruker)
            {
                språkvalg = innloggetBruker.ProfileSettingPreference.Language;
            }
        }
        
        if (språkvalg is { Length: > 0 })
        {
            await _languageObserver.NotifyCurrentLanguage(språkvalg);
        }
    }
    
    public Task ProcessDataWrite(Instance instance, Guid? dataId, object data, object? previousData, string? language)
    {
        return Task.CompletedTask;
    }
}