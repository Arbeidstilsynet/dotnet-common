using Arbeidstilsynet.Common.Saksarkiv.V3.Models.AT.EArkiv.Entiteter.API.V3;
using Arbeidstilsynet.Common.Saksarkiv.V3.Models.Domain.V3.Queue;
using Microsoft.Kiota.Abstractions;

namespace Arbeidstilsynet.Common.Saksarkiv.V3;

/// <summary>
/// Typed convenience extensions over the generated <see cref="SaksarkivClientV3"/>.
/// </summary>
public static class SaksarkivClientV3Extensions
{
    /// <summary>
    /// Creates a new case (sak) using the generated <see cref="OpprettSakRequest"/> model, wrapping
    /// the multipart plumbing required by the generated v3 client.
    /// </summary>
    /// <param name="client">The generated v3 client.</param>
    /// <param name="request">The case-creation payload.</param>
    /// <param name="files">
    /// Files to upload (case documents/attachments). Each file's
    /// <see cref="SaksarkivFile.FileName"/> must match the file names referenced in
    /// <paramref name="request"/> (for example <c>HoveddokumentFilnavn</c> / <c>VedleggFilnavn</c>)
    /// and be unique within the request.
    /// </param>
    /// <param name="avslutt">
    /// When <see langword="true"/>, the case is closed immediately after creation.
    /// </param>
    /// <param name="cancellationToken">Cancellation token to use when cancelling the request.</param>
    /// <returns>
    /// The queued <see cref="MeldingStatus"/> for the asynchronous operation. Track completion via
    /// <c>client.Api.V3.Meldinger[status.MeldingId].GetAsync(...)</c>.
    /// </returns>
    /// <remarks>
    /// The payload is sent as a JSON <c>payload</c> part of a <c>multipart/form-data</c> body,
    /// alongside the uploaded files. This mirrors how the Saksarkiv server reads the request.
    /// </remarks>
    public static async Task<MeldingStatus?> OpprettSakAsync(
        this SaksarkivClientV3 client,
        OpprettSakRequest request,
        IEnumerable<SaksarkivFile>? files = null,
        bool avslutt = false,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);

        var body = BuildOpprettSakBody(request, files);

        return await client.Api.V3.Saker.PostAsync(
            body,
            config => config.QueryParameters.Avslutt = avslutt,
            cancellationToken: cancellationToken
        );
    }

    internal static MultipartBody BuildOpprettSakBody(
        OpprettSakRequest request,
        IEnumerable<SaksarkivFile>? files
    )
    {
        var body = new MultipartBody();
        body.AddOrReplacePart("payload", "application/json", request);

        if (files is not null)
        {
            foreach (var file in files)
            {
                body.AddOrReplacePart(
                    file.FileName,
                    file.ContentType,
                    file.Content,
                    file.FileName
                );
            }
        }

        return body;
    }
}
