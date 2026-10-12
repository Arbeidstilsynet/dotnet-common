using System.Text;
using Microsoft.Kiota.Abstractions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.ErrorReporting;

internal static class ErrorResponseBodyCapture
{
    internal const int MaxBodyLength = 4096;

    internal static async Task<string> CaptureAndRestoreAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var originalContent = response.Content;
        var contentStream = await originalContent.ReadAsStreamAsync(cancellationToken);
        var prefix = await ReadPrefixAsync(contentStream, cancellationToken);
        var restoredContent = new StreamContent(
            new PrefixStream(prefix, contentStream, originalContent)
        );

        foreach (var header in originalContent.Headers)
        {
            restoredContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        response.Content = restoredContent;
        return FormatBody(prefix, originalContent.Headers.ContentType?.CharSet);
    }

    internal static async Task<string> ReadDiagnosticBodyAsync(
        HttpContent content,
        CancellationToken cancellationToken
    )
    {
        var stream = await content.ReadAsStreamAsync(cancellationToken);
        var prefix = await ReadPrefixAsync(stream, cancellationToken);
        return FormatBody(prefix, content.Headers.ContentType?.CharSet);
    }

    private static async Task<byte[]> ReadPrefixAsync(
        Stream stream,
        CancellationToken cancellationToken
    )
    {
        var buffer = new byte[MaxBodyLength + 1];
        var length = 0;

        while (length < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(
                buffer.AsMemory(length, buffer.Length - length),
                cancellationToken
            );
            if (bytesRead == 0)
            {
                break;
            }

            length += bytesRead;
        }

        return buffer.AsSpan(0, length).ToArray();
    }

    private static string FormatBody(byte[] prefix, string? charset)
    {
        var length = Math.Min(prefix.Length, MaxBodyLength);
        var encoding = string.IsNullOrWhiteSpace(charset)
            ? Encoding.UTF8
            : Encoding.GetEncoding(charset.Trim('"'));
        var body = encoding.GetString(prefix, 0, length);

        return prefix.Length > MaxBodyLength ? $"{body}... (truncated)" : body;
    }

    private sealed class PrefixStream(byte[] prefix, Stream contentStream, HttpContent content)
        : Stream
    {
        private readonly MemoryStream _prefix = new(prefix, writable: false);
        private bool _disposed;

        public override bool CanRead => !_disposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _prefix.Read(buffer, offset, count);
            return bytesRead > 0 ? bytesRead : contentStream.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            var bytesRead = _prefix.Read(buffer);
            return bytesRead > 0 ? bytesRead : contentStream.Read(buffer);
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken
        )
        {
            var bytesRead = _prefix.Read(buffer, offset, count);
            return bytesRead > 0
                ? Task.FromResult(bytesRead)
                : contentStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
        )
        {
            var bytesRead = _prefix.Read(buffer.Span);
            return bytesRead > 0
                ? ValueTask.FromResult(bytesRead)
                : contentStream.ReadAsync(buffer, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _prefix.Dispose();
                try
                {
                    contentStream.Dispose();
                }
                finally
                {
                    content.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}

/// <summary>
/// Carries the body of an error response from <see cref="AltinnErrorResponseCaptureHandler"/>
/// back to <see cref="AltinnErrorReportingRequestAdapter"/>.
/// </summary>
/// <remarks>
/// Kiota copies request options onto the outgoing <see cref="HttpRequestMessage"/>, which is the
/// only channel between the request adapter and the HTTP pipeline for a single request.
/// </remarks>
internal sealed class AltinnErrorResponseCapture : IRequestOption
{
    public static readonly HttpRequestOptionsKey<IRequestOption> Key = new(
        typeof(AltinnErrorResponseCapture).FullName!
    );

    public string? Body { get; set; }
}

/// <summary>
/// Records the body of an error response, so that it survives even when the specification
/// declares no error type for the status code and Kiota would otherwise discard it.
/// </summary>
internal sealed class AltinnErrorResponseCaptureHandler : DelegatingHandler
{
    internal const int MaxBodyLength = ErrorResponseBodyCapture.MaxBodyLength;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (
            response.IsSuccessStatusCode
            || !request.Options.TryGetValue(AltinnErrorResponseCapture.Key, out var option)
            || option is not AltinnErrorResponseCapture capture
        )
        {
            return response;
        }

        capture.Body = await ErrorResponseBodyCapture.CaptureAndRestoreAsync(
            response,
            cancellationToken
        );

        return response;
    }
}
