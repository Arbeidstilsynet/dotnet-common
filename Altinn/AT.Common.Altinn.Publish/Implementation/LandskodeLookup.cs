using System.Reflection;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model;
using Arbeidstilsynet.Common.Altinn.Ports;
using Newtonsoft.Json;
using AssemblyExtensions = Arbeidstilsynet.Common.Altinn.Extensions.AssemblyExtensions;

namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal class LandskodeLookup : ILandskodeLookup
{
    private const string Filename = "landskoder.json";
    private Dictionary<string, Landskode>? _landskoder;

    public async Task<Landskode?> GetLandskode(string landkode)
    {
        _landskoder ??= await IngestAsync();

        return _landskoder.GetValueOrDefault(landkode);
    }

    public async Task<IEnumerable<KeyValuePair<string, Landskode>>> GetLandskoder()
    {
        _landskoder ??= await IngestAsync();

        return _landskoder;
    }
    
    private static Task<Dictionary<string, Landskode>> IngestAsync()
    {
        return Assembly.GetExecutingAssembly().GetEmbeddedResource<Dictionary<string, Landskode>>(Filename);
    }
}
