using WireMock.Admin;
using WireMock.Admin.Mappings;
using WireMock.Net.OpenApiParser;
using WireMock.Net.OpenApiParser.Settings;
using WireMock.Server;

namespace Arbeidstilsynet.Common.TestExtensions.Extensions;

/// <summary>
/// Extensions for TestExtensions
/// </summary>
public static class WireMockExtensions
{
    /// <summary>
    /// Adds OpenAPI mappings to the WireMock server from a stream containing the OpenAPI specification.
    ///
    /// By default it will generate example responses based on the schema definitions in the OpenAPI specification.
    /// </summary>
    /// <remarks>
    /// If an endpoint returns "oneOf" in the response schema, it will not generate mappings for the return values on those endpoints.
    /// </remarks>
    /// <param name="server"></param>
    /// <param name="openApiSpecStream">A stream containing the json open api spec</param>
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

        parserSettings ??= new WireMockOpenApiParserSettings
        {
            ExampleValues = new ExampleValuesGenerator(),
        };

        var parser = new WireMockOpenApiParser();
        var mappings = parser.FromStream(openApiSpecStream, parserSettings, out _);

        server.WithMapping(mappings.Select(mappingVisitor).ToArray());
    }
}

file class ExampleValuesGenerator : WireMockOpenApiParserExampleValues
{
    private readonly DateTime _dateTime = System.DateTime.UtcNow;
    public override Func<DateTime> Date => () => _dateTime.Date;
    public override Func<DateTime> DateTime => () => _dateTime;
}
