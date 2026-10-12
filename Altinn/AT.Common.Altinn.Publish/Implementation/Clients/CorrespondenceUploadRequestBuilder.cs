using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Arbeidstilsynet.Common.Altinn.Correspondence.Correspondence.Api.V1.Correspondence.Upload;

// Extends the generated builder only for duplicate-name multipart parts that MultipartBody
// cannot represent. Retains its route and request adapter (auth, resilience and error capture).
internal partial class UploadRequestBuilder
{
    public async Task<InitializeCorrespondencesResponseExt?> PostAsync(
        MultipartFormDataContent body,
        CancellationToken cancellationToken = default
    )
    {
        // As with Kiota's multipart writer, buffer the serialization so retries can rewind it,
        // including when an incoming file stream is not seekable.
        using var content = new MemoryStream();
        await body.CopyToAsync(content, cancellationToken);
        content.Position = 0;
        var request = new RequestInformation(Method.POST, UrlTemplate, PathParameters);
        request.Headers.TryAdd("Accept", "application/json");
        request.SetStreamContent(content, body.Headers.ContentType!.ToString());

        // The upload specification's declared errors, matching the generated overload.
        Dictionary<string, ParsableFactory<IParsable>> errors = new()
        {
            ["401"] = ProblemDetails.CreateFromDiscriminatorValue,
            ["403"] = ProblemDetails.CreateFromDiscriminatorValue,
            ["404"] = ProblemDetails.CreateFromDiscriminatorValue,
            ["408"] = ProblemDetails.CreateFromDiscriminatorValue,
            ["422"] = ProblemDetails.CreateFromDiscriminatorValue,
        };
        return await RequestAdapter.SendAsync(
            request,
            InitializeCorrespondencesResponseExt.CreateFromDiscriminatorValue,
            errors,
            cancellationToken
        );
    }
}
