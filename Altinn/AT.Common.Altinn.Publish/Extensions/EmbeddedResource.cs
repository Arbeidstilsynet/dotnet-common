using System.Reflection;
using Newtonsoft.Json;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

/// <summary>
/// Provides methods to read and deserialize embedded JSON resources from an assembly.
/// </summary>
public static class EmbeddedResource
{
    /// <summary>
    /// Reads an embedded JSON resource and deserializes it into the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="fileName">The file name of the embedded resource, including the extension (e.g., "data.json").</param>
    /// <param name="assemblyWithEmbeddedResource">Optional assembly to search for the embedded resource. If not provided, the assembly containing the type <typeparamref name="T"/> will be used.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="Exception"></exception>
    public static async Task<T> Ingest<T>(string fileName, Assembly? assemblyWithEmbeddedResource = null)
        where T : class
    {
        assemblyWithEmbeddedResource ??= typeof(T).Assembly;
        
        var resourceName = assemblyWithEmbeddedResource.GetManifestResourceNames().SingleOrDefault(str => str.EndsWith(fileName)) 
                           ?? throw new Exception($"Could not find a singular resource: {fileName}");
        
        await using var stream = assemblyWithEmbeddedResource.GetManifestResourceStream(resourceName) 
                           ?? throw new Exception($"Failed to get stream for embedded resource: {fileName}");
        
        using var reader = new StreamReader(stream);
        
        var json = await reader.ReadToEndAsync();

        return JsonConvert.DeserializeObject<T>(json) 
               ?? throw new Exception($"Error deserializing json from embedded resource: {fileName}"); 
    }
}