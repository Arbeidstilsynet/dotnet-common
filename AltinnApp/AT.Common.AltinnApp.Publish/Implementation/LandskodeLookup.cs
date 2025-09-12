using System.Reflection;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal class LandskodeLookup : ILandskodeLookup
{
    private const string Filename = "landskoder.json";
    private Dictionary<string, Landskode>? _landskoder;

    public async Task<Landskode?> GetLandskode(string isoCode)
    {
        _landskoder ??= await IngestAsync();

        return _landskoder.GetValueOrDefault(isoCode);
    }

    public async Task<IEnumerable<KeyValuePair<string, Landskode>>> GetLandskoder()
    {
        _landskoder ??= await IngestAsync();

        return _landskoder;
    }

    private static Task<Dictionary<string, Landskode>> IngestAsync()
    {
        return Assembly
            .GetExecutingAssembly()
            .GetEmbeddedResource<Dictionary<string, Landskode>>(Filename);
    }
}
