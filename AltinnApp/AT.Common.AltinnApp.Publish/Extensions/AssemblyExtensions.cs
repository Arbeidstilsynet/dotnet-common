using System.Reflection;
using System.Text.Json;

namespace Arbeidstilsynet.Common.AltinnApp.Extensions;

/// <summary>
/// Provides methods to read and deserialize embedded JSON resources from an assembly.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Reads an embedded JSON resource and deserializes it into the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="assemblyWithEmbeddedResource">Assembly to search for the embedded resource.</param>
    /// <param name="fileName">The file name of the embedded resource, including the extension (e.g., "data.json").</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="Exception"></exception>
    public static async Task<T> GetEmbeddedResource<T>(
        this Assembly assemblyWithEmbeddedResource,
        string fileName
    )
        where T : class
    {
        var resourceName =
            assemblyWithEmbeddedResource
                .GetManifestResourceNames()
                .SingleOrDefault(str => str.EndsWith(fileName))
            ?? throw new Exception($"Could not find a singular resource: {fileName}");

        await using var stream =
            assemblyWithEmbeddedResource.GetManifestResourceStream(resourceName)
            ?? throw new Exception($"Failed to get stream for embedded resource: {fileName}");

        using var reader = new StreamReader(stream);

        var json = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize<T>(json)
            ?? throw new Exception($"Error deserializing json from embedded resource: {fileName}");
    }
}
