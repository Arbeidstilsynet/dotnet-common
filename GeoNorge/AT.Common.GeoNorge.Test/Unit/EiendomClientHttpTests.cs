using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using Arbeidstilsynet.Common.GeoNorge.DependencyInjection;
using Arbeidstilsynet.Common.GeoNorge.Eiendom;
using Arbeidstilsynet.Common.GeoNorge.Eiendom.Models;
using Arbeidstilsynet.Common.GeoNorge.Implementation;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Serialization.Json;
using NSubstitute;
using Shouldly;
using GeokodingQueryParameters = Arbeidstilsynet.Common.GeoNorge.Eiendom.Geokoding.GeokodingRequestBuilder.GeokodingRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Test.Unit;

public class EiendomClientHttpTests
{
    // Representative-point regression captured for addressless property 5512/2/16 on 2026-10-05.
    private const string TwoParcels = """
        {
          "type": "FeatureCollection",
          "features": [
            {
              "type": "Feature",
              "geometry": { "type": "Point", "coordinates": [16.82662, 68.68422] },
              "properties": {
                "kommunenummer": "5512", "gardsnummer": 2, "bruksnummer": 16,
                "festenummer": 0, "seksjonsnummer": 0, "hovedomr\u00e5de": true
              }
            },
            {
              "type": "Feature",
              "geometry": { "type": "Point", "coordinates": [16.82759, 68.68239] },
              "properties": {
                "kommunenummer": "5512", "gardsnummer": 2, "bruksnummer": 16,
                "festenummer": 0, "seksjonsnummer": 0, "hovedomr\u00e5de": false
              }
            }
          ]
        }
        """;

    private const string EmptyCollection = """{"type":"FeatureCollection","features":[]}""";

    [Fact]
    public async Task SearchProperties_SerializesIdentifiersAndDefaultCoordinatesWithoutAuthentication()
    {
        using var fixture = new HttpFixture();
        var query = new GeokodingQueryParameters
        {
            Kommunenummer = "0301",
            Gardsnummer = 223,
            Bruksnummer = 60,
            Festenummer = 0,
            Seksjonsnummer = 3,
        };

        await fixture.Client.SearchProperties(query);

        fixture
            .Handler.RequestUri.ShouldNotBeNull()
            .GetLeftPart(UriPartial.Path)
            .ShouldBe("https://api.kartverket.no/eiendom/v1/geokoding");
        fixture.Handler.Method.ShouldBe(HttpMethod.Get);
        fixture.Handler.Authorization.ShouldBeNull();
        fixture.Handler.Accept.ShouldBe("application/json");
        var parameters = HttpUtility.ParseQueryString(fixture.Handler.RequestUri.Query);
        parameters.Count.ShouldBe(6);
        parameters["kommunenummer"].ShouldBe("0301");
        parameters["gardsnummer"].ShouldBe("223");
        parameters["bruksnummer"].ShouldBe("60");
        parameters["festenummer"].ShouldBe("0");
        parameters["seksjonsnummer"].ShouldBe("3");
        parameters["utkoordsys"].ShouldBe("4326");
    }

    [Theory]
    [InlineData(4258)]
    [InlineData(25833)]
    public async Task SearchProperties_SerializesMatrikkelnummerAndExplicitCoordinateSystem(
        int coordinateSystem
    )
    {
        using var fixture = new HttpFixture();

        await fixture.Client.SearchProperties(
            new GeokodingQueryParameters
            {
                Matrikkelnummer = "0301-223/60/0/3",
                Utkoordsys = coordinateSystem,
                Filtrer = "features.geometry,features.properties",
            }
        );

        var uri = fixture.Handler.RequestUri.ShouldNotBeNull();
        uri.Query.ShouldContain("matrikkelnummer=0301-223%2F60%2F0%2F3");
        var parameters = HttpUtility.ParseQueryString(uri.Query);
        parameters.Count.ShouldBe(3);
        parameters["matrikkelnummer"].ShouldBe("0301-223/60/0/3");
        parameters["utkoordsys"].ShouldBe(coordinateSystem.ToString());
        parameters["filtrer"].ShouldBe("features.geometry,features.properties");
    }

    [Fact]
    public async Task SearchProperties_AddresslessPropertyPreservesBothParcelsAndLongitudeLatitudeOrder()
    {
        using var fixture = new HttpFixture(TwoParcels);

        var result = await fixture.Client.SearchProperties(
            new GeokodingQueryParameters
            {
                Kommunenummer = "5512",
                Gardsnummer = 2,
                Bruksnummer = 16,
            }
        );

        var response = result.ShouldNotBeNull();
        response.Type.ShouldBe("FeatureCollection");
        var features = response.Features.ShouldNotBeNull();
        features.Count.ShouldBe(2);
        foreach (var feature in features)
        {
            feature.Type.ShouldBe("Feature");
            feature.Geometry.ShouldNotBeNull().Type.ShouldBe("Point");
            var properties = feature.Properties.ShouldNotBeNull();
            properties.Kommunenummer.ShouldBe("5512");
            properties.Gardsnummer.ShouldBe(2);
            properties.Bruksnummer.ShouldBe(16);
            properties.Festenummer.ShouldBe(0);
            properties.Seksjonsnummer.ShouldBe(0);
        }
        features[0].Properties.ShouldNotBeNull().Hovedområde.ShouldBe(true);
        features[1].Properties.ShouldNotBeNull().Hovedområde.ShouldBe(false);

        using var json = await SerializeResponse(response);
        var parcels = json.RootElement.GetProperty("features");
        AssertPoint(parcels[0], 16.82662, 68.68422);
        AssertPoint(parcels[1], 16.82759, 68.68239);
    }

    [Fact]
    public async Task SearchProperties_AreaRequestPreservesNestedGeometryAndExplicitCrs()
    {
        const string polygon = """
            {
              "type": "FeatureCollection",
              "crs": { "type": "name", "properties": { "name": "EPSG:25833" } },
              "features": [{
                "type": "Feature",
                "properties": { "kommunenummer": "0301", "hovedomr\u00e5de": true },
                "geometry": {
                  "type": "Polygon",
                  "coordinates": [[[262000, 6650000], [262010, 6650000],
                                   [262010, 6650010], [262000, 6650000]]]
                }
              }]
            }
            """;
        using var fixture = new HttpFixture(polygon);

        var result = await fixture.Client.SearchProperties(
            new GeokodingQueryParameters
            {
                Matrikkelnummer = "0301-223/60",
                Omrade = true,
                Utkoordsys = 25833,
            }
        );

        var parameters = HttpUtility.ParseQueryString(
            fixture.Handler.RequestUri.ShouldNotBeNull().Query
        );
        parameters["omrade"].ShouldBe("true");
        parameters["utkoordsys"].ShouldBe("25833");
        var response = result.ShouldNotBeNull();
        response.Crs.ShouldNotBeNull().Properties.ShouldNotBeNull().Name.ShouldBe("EPSG:25833");
        var feature = response.Features.ShouldNotBeNull().ShouldHaveSingleItem();
        feature.Properties.ShouldNotBeNull().Kommunenummer.ShouldBe("0301");
        feature.Geometry.ShouldNotBeNull().Type.ShouldBe("Polygon");

        using var json = await SerializeResponse(response);
        var ring = json
            .RootElement.GetProperty("features")[0]
            .GetProperty("geometry")
            .GetProperty("coordinates")[0];
        ring.GetArrayLength().ShouldBe(4);
        ring[0][0].GetDouble().ShouldBe(262000);
        ring[0][1].GetDouble().ShouldBe(6650000);
        ring[2][0].GetDouble().ShouldBe(262010);
        ring[2][1].GetDouble().ShouldBe(6650010);
        ring[3].GetRawText().ShouldBe(ring[0].GetRawText());
    }

    [Fact]
    public async Task SearchProperties_EmptySuccessIsDistinctFromFailure()
    {
        using var fixture = new HttpFixture();

        var result = await fixture.Client.SearchProperties(new GeokodingQueryParameters());

        result.ShouldNotBeNull().Features.ShouldNotBeNull().ShouldBeEmpty();
        result.Crs.ShouldBeNull();
        fixture.Logger.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(500)]
    public async Task SearchProperties_HttpFailureReturnsNullAndLogsWarning(int status)
    {
        using var fixture = new HttpFixture("""{"error":"Upstream failure"}""");
        fixture.Handler.StatusCode = (HttpStatusCode)status;

        var result = await fixture.Client.SearchProperties(new GeokodingQueryParameters());

        result.ShouldBeNull();
        fixture
            .Logger.ReceivedCalls()
            .ShouldHaveSingleItem()
            .GetArguments()[0]
            .ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public async Task SearchProperties_NetworkFailureReturnsNullAndLogsWarning()
    {
        using var fixture = new HttpFixture();
        fixture.Handler.Failure = new HttpRequestException("Network unavailable");

        var result = await fixture.Client.SearchProperties(new GeokodingQueryParameters());

        result.ShouldBeNull();
        fixture
            .Logger.ReceivedCalls()
            .ShouldHaveSingleItem()
            .GetArguments()[0]
            .ShouldBe(LogLevel.Warning);
    }

    [Fact]
    public async Task SearchProperties_MalformedJsonPropagatesRatherThanReturningEmptySuccess()
    {
        using var fixture = new HttpFixture("{invalid");

        await Should.ThrowAsync<JsonException>(() =>
            fixture.Client.SearchProperties(new GeokodingQueryParameters())
        );

        fixture.Logger.ReceivedCalls().ShouldBeEmpty();
    }

    private static void AssertPoint(JsonElement feature, double longitude, double latitude)
    {
        var coordinates = feature.GetProperty("geometry").GetProperty("coordinates");
        coordinates.GetArrayLength().ShouldBe(2);
        coordinates[0].GetDouble().ShouldBe(longitude);
        coordinates[1].GetDouble().ShouldBe(latitude);
    }

    private static async Task<JsonDocument> SerializeResponse(GeoKodingRespons response)
    {
        using var writer = new JsonSerializationWriter();
        writer.WriteObjectValue(null, response);
        using var stream = writer.GetSerializedContent();
        return await JsonDocument.ParseAsync(stream);
    }

    private sealed class HttpFixture : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly EiendomRequestAdapter _adapter;

        public StubHandler Handler { get; }
        public PropertySearchClient Client { get; }
        public ILogger<PropertySearchClient> Logger { get; } =
            Substitute.For<ILogger<PropertySearchClient>>();

        public HttpFixture(string body = EmptyCollection)
        {
            Handler = new StubHandler(body);
            _httpClient = new HttpClient(Handler);
            var factory = Substitute.For<IHttpClientFactory>();
            factory
                .CreateClient(DependencyInjectionExtensions.EiendomHttpClientName)
                .Returns(_httpClient);
            _adapter = new EiendomRequestAdapter(factory);
            Client = new PropertySearchClient(new EiendomClient(_adapter), Logger);
        }

        public void Dispose()
        {
            _adapter.Dispose();
            _httpClient.Dispose();
        }
    }

    private sealed class StubHandler(string body) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? Authorization { get; private set; }
        public string? Accept { get; private set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Exception? Failure { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestUri = request.RequestUri;
            Method = request.Method;
            Authorization = request.Headers.Authorization?.ToString();
            Accept = request.Headers.Accept.ToString();
            return Failure is { } exception
                ? Task.FromException<HttpResponseMessage>(exception)
                : Task.FromResult(
                    new HttpResponseMessage(StatusCode)
                    {
                        Content = new StringContent(body, Encoding.UTF8, "application/json"),
                    }
                );
        }
    }
}
