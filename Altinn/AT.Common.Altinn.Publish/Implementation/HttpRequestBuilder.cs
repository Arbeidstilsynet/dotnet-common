namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal class HttpRequestBuilder
{
    private bool _isSent = false;

    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _request;

    private List<string> _queryParameters = [];

    public HttpRequestBuilder(HttpClient httpClient, HttpRequestMessage request)
    {
        _httpClient = httpClient;
        _request = request;
    }

    public HttpRequestBuilder WithHeader(string name, string value)
    {
        _request.Headers.Add(name, value);
        return this;
    }

    public HttpRequestBuilder WithQueryParameter(string name, string? value)
    {
        if (value != null)
        {
            _queryParameters.Add($"{name}={value}");
        }

        return this;
    }

    public HttpRequestBuilder WithQueryParameterArray(string name, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            _queryParameters.Add($"{name}={value}");
        }

        return this;
    }

    public Task<HttpResponseMessage> Send()
    {
        if (_isSent)
        {
            throw new InvalidOperationException("Request has already been sent");
        }

        _isSent = true;

        if (_queryParameters.Count != 0)
        {
            _request.RequestUri = new Uri(
                $"{_request.RequestUri}?{string.Join('&', _queryParameters)}",
                UriKind.Relative
            );
        }

        return _httpClient.SendAsync(_request);
    }
}