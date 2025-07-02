using System.Reflection;
using Arbeidstilsynet.Common.Altinn.Model;
using Newtonsoft.Json;

namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal class LandskodeLookup : ILandskodeLookup
{
    private const string Filename = "landskoder.json";
    private Dictionary<string, Landskode>? _landskoder;

    public async Task<Landskode?> GetLandskode(string landkode)
    {
        _landskoder ??= await IngestAsync();

        return _landskoder.TryGetValue(landkode, out var landskode) ? landskode : null;
    }

    private static async Task<Dictionary<string, Landskode>> IngestAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fileName = assembly.GetManifestResourceNames().SingleOrDefault(str => str.EndsWith(Filename));
        if (fileName is null)
        {
            return [];
        }

        await using var stream = assembly.GetManifestResourceStream(fileName);
        if (stream is null)
        {
            return [];
        }
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        return JsonConvert.DeserializeObject<Dictionary<string, Landskode>>(json) ?? [];
    }

    public async Task<IEnumerable<KeyValuePair<string, Landskode>>> GetLandskoder()
    {
        _landskoder ??= await IngestAsync();

        return _landskoder;
    }
}
