using WireMock.Admin.Mappings;
using WireMock.Net.OpenApiParser;
using WireMock.Net.OpenApiParser.Settings;
using WireMock.Server;

namespace AT.Common.GeoNorge.Test.Extensions;

public static class WireMockExtensions
{
    /// <summary>
    /// Adds OpenAPI mappings to the WireMock server from a stream containing the OpenAPI specification.
    ///
    /// By default it will generate example responses based on the schema definitions in the OpenAPI specification.
    ///
    /// Known limitations:
    /// - If an endpoint returns "oneOf" in the response schema, it will not generate mappings for the return values on those endpoints.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="openApiSpecStream"></param>
    /// <param name="mappingVisitor"></param>
    /// <param name="parserSettings"></param>
    public static void AddOpenApiMappings(
        this WireMockServer server,
        Stream openApiSpecStream,
        Func<MappingModel, MappingModel>? mappingVisitor = null,
        WireMockOpenApiParserSettings? parserSettings = null
    )
    {
        mappingVisitor ??= m => m;

        var settings =
            parserSettings
            ?? new WireMockOpenApiParserSettings
            {
                // Generate example responses based on schema
                ExampleValues = new WireMockOpenApiParserExampleValues(),
            };

        var parser = new WireMockOpenApiParser();
        var mappings = parser.FromStream(openApiSpecStream, settings, out _);

        server.WithMapping(mappings.Select(mappingVisitor).ToArray());
    }
}
