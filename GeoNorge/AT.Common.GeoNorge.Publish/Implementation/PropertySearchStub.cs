using Arbeidstilsynet.Common.GeoNorge.Eiendom.Models;
using Arbeidstilsynet.Common.GeoNorge.Ports;
using Arbeidstilsynet.Shared.Implementation;
using Microsoft.Kiota.Abstractions.Serialization;
using GeokodingQueryParameters = Arbeidstilsynet.Common.GeoNorge.Eiendom.Geokoding.GeokodingRequestBuilder.GeokodingRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.GeoNorge.Implementation;

internal sealed class PropertySearchStub : IPropertySearch
{
    public Task<GeoKodingRespons?> SearchProperties(GeokodingQueryParameters queryParameters)
    {
        StubQuery.Validate(
            queryParameters,
            "Matrikkelnummer",
            "Kommunenummer",
            "Gardsnummer",
            "Bruksnummer",
            "Utkoordsys",
            "Omrade"
        );
        StubQuery.GeographicCoordinates(queryParameters.Utkoordsys);
        if (queryParameters.Omrade == true)
        {
            throw new NotSupportedException(
                "The Development property stub only provides point geometries."
            );
        }

        var matches =
            (queryParameters.Matrikkelnummer is null or "0301-1/1")
            && (queryParameters.Kommunenummer is null or "0301")
            && (queryParameters.Gardsnummer is null or 1)
            && (queryParameters.Bruksnummer is null or 1);
        return Task.FromResult<GeoKodingRespons?>(
            new()
            {
                Type = "FeatureCollection",
                Features = matches
                    ?
                    [
                        new()
                        {
                            Type = "Feature",
                            Properties = new()
                            {
                                Kommunenummer = "0301",
                                Gardsnummer = 1,
                                Bruksnummer = 1,
                                Matrikkelnummertekst = "0301-1/1",
                            },
                            Geometry = new()
                            {
                                Type = "Point",
                                Coordinates = new UntypedArray([
                                    new UntypedDouble(10.7522),
                                    new UntypedDouble(59.9139),
                                ]),
                            },
                        },
                    ]
                    : [],
            }
        );
    }
}
