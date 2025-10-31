using Altinn.App.Core.Features;
using Altinn.App.Core.Models;
using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal class LandOptions : IAppOptionsProvider
{
    public string Id { get; }
    private readonly ILandskodeLookup _landskodeLookup;

    private readonly Func<IEnumerable<Landskode>, IEnumerable<Landskode>> _orderFunc;
    private readonly LandOptionsConfiguration.IsoType _optionValueIsoType;

    public LandOptions(
        ILandskodeLookup landskodeLookup,
        IOptions<LandOptionsConfiguration> landOptionsConfiguration
    )
    {
        _landskodeLookup = landskodeLookup;
        Id = landOptionsConfiguration.Value.OptionsId;
        _orderFunc = landOptionsConfiguration.Value.CustomOrderFunc ?? (l => l);
        _optionValueIsoType = landOptionsConfiguration.Value.OptionValueIsoType;
    }

    public async Task<AppOptions> GetAppOptionsAsync(
        string? language,
        Dictionary<string, string> keyValuePairs
    )
    {
        var landskoder = new List<AppOption>();

        foreach (var landData in _orderFunc((await _landskodeLookup.GetLandskoder()).Select(kvp => kvp.Value)))
        {
            var value = _optionValueIsoType switch
            {
                LandOptionsConfiguration.IsoType.Alpha2 => landData.Alpha2,
                LandOptionsConfiguration.IsoType.Alpha3 => landData.Alpha3,
                _ => throw new NotSupportedException($"Unsupported ISO type: {_optionValueIsoType}")
            };
            
            landskoder.Add(new AppOption { Label = landData.Land, Value = value });
        }

        return new AppOptions { Options = landskoder, IsCacheable = true };
    }
}
