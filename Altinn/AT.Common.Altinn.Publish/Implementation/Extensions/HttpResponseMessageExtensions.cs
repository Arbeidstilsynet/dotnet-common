using Arbeidstilsynet.Common.Altinn.Model.Exceptions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Extensions;

internal static class HttpResponseMessageExtensions
{
    public static async Task EnsureSuccessStatusCodeWithBody(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();

        throw new AltinnHttpRequestException(
            response.RequestMessage,
            response.StatusCode,
            body,
            $"Request to AltinAPI failed. HTTP {(int)response.StatusCode} {response.StatusCode} from "
                + $"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}. "
                + $"Response body: {body}"
        );
    }
}
