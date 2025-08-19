using System.Net.Http.Headers;

namespace Arbeidstilsynet.Common.Altinn.Implementation;

internal interface IHttpRequestBuilder
{
    Task<HttpResponseMessage> Send();
    IHttpRequestBuilder WithHeader(string name, string value);

    IHttpRequestBuilder WithAcceptHeader(string value, double quality);
    IHttpRequestBuilder WithQueryParameter(string name, string? value);
    IHttpRequestBuilder WithQueryParameterArray(string name, IEnumerable<string> values);
}

internal class HttpRequestBuilder : IHttpRequestBuilder
{
    private bool _isSent = false;

    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _request;

    private readonly List<string> _queryParameters = [];

    public HttpRequestBuilder(HttpClient httpClient, HttpRequestMessage request)
    {
        _httpClient = httpClient;
        _request = request;
    }

    public IHttpRequestBuilder WithHeader(string name, string value)
    {
        _request.Headers.Add(name, value);
        return this;
    }

    public IHttpRequestBuilder WithQueryParameter(string name, string? value)
    {
        if (value != null)
        {
            _queryParameters.Add($"{name}={value}");
        }

        return this;
    }

    public IHttpRequestBuilder WithQueryParameterArray(string name, IEnumerable<string> values)
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
            throw new InvalidOperationException(
                "Request has already been sent. The HttpRequestBuilder instance cannot be reused. Create a new HttpRequestBuilder instance for additional requests."
            );
        }

        _isSent = true;

        if (_queryParameters.Count != 0)
        {
            // Default to relative
            var kind =
                _request.RequestUri?.IsAbsoluteUri ?? false ? UriKind.Absolute : UriKind.Relative;

            _request.RequestUri = new Uri(
                $"{_request.RequestUri}?{string.Join('&', _queryParameters)}",
                kind
            );
        }

        return _httpClient.SendAsync(_request);
    }

    public IHttpRequestBuilder WithAcceptHeader(string value, double quality)
    {
        _request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(value, quality));
        return this;
    }
}
