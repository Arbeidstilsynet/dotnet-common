using System.Reflection;
using GeneratedInstanceQueryParameters = Arbeidstilsynet.Common.Altinn.Storage.Instances.InstancesRequestBuilder.InstancesRequestBuilderGetQueryParameters;

namespace Arbeidstilsynet.Common.Altinn.Model.Api.Request;

/// <summary>
/// The parameters for an instance query: the generated query parameters, plus the parameters Altinn
/// expects outside the query string.
/// </summary>
/// <remarks>
/// <para>
/// The generated <see cref="GeneratedInstanceQueryParameters"/> is used directly rather than
/// mirrored, so a parameter added to the storage specification becomes available as soon as the
/// client is regenerated.
/// </para>
/// <para>
/// It cannot be used on its own because Kiota omits header parameters from the classes it generates
/// for query parameters, and the storage API declares
/// <c>X-Ai-InstanceOwnerIdentifier</c> as a header. This type carries that header alongside them.
/// </para>
/// <para>
/// Note that <see cref="Parameters"/> is a generated class rather than a record, so although this
/// type is a record, two instances holding equivalent but distinct parameter objects do not compare
/// equal. Use <see cref="WithContinuationToken"/> rather than <c>with</c> when paging, so that the
/// parameters are copied rather than shared with the caller.
/// </para>
/// </remarks>
public record InstanceQuery
{
    internal const string InstanceOwnerIdentifierHeaderName = "X-Ai-InstanceOwnerIdentifier";
    internal const string ContinuationTokenParameterName = "continuationToken";

    /// <summary>
    /// The query parameters, as generated from the storage specification.
    /// </summary>
    public GeneratedInstanceQueryParameters Parameters { get; init; } = new();

    /// <summary>
    /// The instance owner identifier, sent as the <c>X-Ai-InstanceOwnerIdentifier</c> header.
    /// </summary>
    public string? InstanceOwnerIdentifier { get; init; }

    /// <summary>
    /// Creates a query from the given generated query parameters.
    /// </summary>
    public static implicit operator InstanceQuery(GeneratedInstanceQueryParameters parameters) =>
        new() { Parameters = parameters };

    /// <summary>
    /// Returns a copy of this query carrying the given continuation token.
    /// </summary>
    /// <remarks>
    /// The generated query parameters are a mutable class, so they are copied rather than shared.
    /// Paging would otherwise write the continuation token into the object the caller passed in.
    /// </remarks>
    internal InstanceQuery WithContinuationToken(string continuationToken)
    {
        var parameters = CopyParameters(Parameters);

        parameters.ContinuationToken = continuationToken;

        return this with
        {
            Parameters = parameters,
        };
    }

    private static GeneratedInstanceQueryParameters CopyParameters(
        GeneratedInstanceQueryParameters source
    )
    {
        var copy = new GeneratedInstanceQueryParameters();

        // Every generated query parameter is a primitive, a string or an array of strings, so
        // copying the property values is sufficient.
        foreach (
            var property in typeof(GeneratedInstanceQueryParameters).GetProperties(
                BindingFlags.Public | BindingFlags.Instance
            )
        )
        {
            if (property.CanRead && property.CanWrite)
            {
                property.SetValue(copy, property.GetValue(source));
            }
        }

        return copy;
    }
}
