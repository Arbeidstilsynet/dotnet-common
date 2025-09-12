using Altinn.App.Core.Features;
using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal class LandOptions : IAppOptionsProvider
{
    public string Id { get; }
    private readonly ILandskodeLookup _landskodeLookup;

    public LandOptions(
        ILandskodeLookup landskodeLookup,
        IOptions<LandOptionsConfiguration> landOptionsConfiguration
    )
    {
        _landskodeLookup = landskodeLookup;
        Id = landOptionsConfiguration.Value.OptionsId;
    }

    public async Task<AppOptions> GetAppOptionsAsync(
        string? language,
        Dictionary<string, string> keyValuePairs
    )
    {
        var landskoder = new List<AppOption>();

        foreach (var (landISOCode, land) in await _landskodeLookup.GetLandskoder())
        {
            landskoder.Add(new AppOption { Label = land.Land, Value = landISOCode });
        }

        return new AppOptions { Options = landskoder, IsCacheable = true };
    }
}
