using System.Net;
using Arbeidstilsynet.Common.AspNetCore.DependencyInjection;
using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

public class MemoryCachingHandlerTests
{
    private readonly IOptions<CachingOptions> _cachingOptions = Options.Create(
        new CachingOptions()
        {
            SlidingExpiration = TimeSpan.FromMinutes(5),
            AbsoluteExpiration = TimeSpan.FromHours(1),
        }
    );

    private readonly VerifySettings _verifySettings = new();

    public MemoryCachingHandlerTests()
    {
        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    [Fact]
    public async Task SendAsync_CachesSuccessfulGetResponse()
    {
        // Arrange
        var cache = Substitute.For<IMemoryCache>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("response"),
        };
        var handler = new MemoryCachingHandler(cache, _cachingOptions)
        {
            InnerHandler = new TestHandler(responseMessage),
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        // Act
        _ = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        cache.Received(1).Set(request.RequestUri!.ToString(), responseMessage);
    }

    [Fact]
    public async Task SendAsync_DoesNotCache_NonGetRequests()
    {
        var cache = Substitute.For<IMemoryCache>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new MemoryCachingHandler(cache, _cachingOptions)
        {
            InnerHandler = new TestHandler(responseMessage),
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://test/api");

        _ = await invoker.SendAsync(request, CancellationToken.None);

        cache.DidNotReceiveWithAnyArgs().Set<HttpResponseMessage>(default!, default!);
    }

    [Fact]
    public async Task SendAsync_ReturnsCachedResponse_WhenCacheHit()
    {
        var cache = Substitute.For<IMemoryCache>();
        var cachedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("cached"),
        };

        cache
            .TryGetValue(Arg.Any<object>(), out Arg.Any<object?>())
            .Returns(x =>
            {
                x[1] = cachedResponse;
                return true;
            });

        var handler = new MemoryCachingHandler(cache, _cachingOptions)
        {
            InnerHandler = new TestHandler(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("should not be used"),
                }
            ),
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        var response = await invoker.SendAsync(request, CancellationToken.None);

        await Verify(response, _verifySettings);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldBe("cached");
    }

    [Fact]
    public async Task SendAsync_DoesNotCache_UnsuccessfulResponse()
    {
        var cache = Substitute.For<IMemoryCache>();
        var handler = new MemoryCachingHandler(cache, _cachingOptions)
        {
            InnerHandler = new TestHandler(new HttpResponseMessage(HttpStatusCode.BadRequest)),
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        _ = await invoker.SendAsync(request, CancellationToken.None);

        cache.DidNotReceiveWithAnyArgs().Set<HttpResponseMessage>(default!, default!);
    }

    [Fact]
    public async Task SendAsync_UsesCachingOptions()
    {
        // Arrange
        var cache = Substitute.For<IMemoryCache>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("response"),
        };
        var handler = new MemoryCachingHandler(cache, _cachingOptions)
        {
            InnerHandler = new TestHandler(responseMessage),
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        // Act
        _ = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        cache
            .Received(1)
            .Set(
                request.RequestUri!.ToString(),
                responseMessage,
                new MemoryCacheEntryOptions()
                {
                    SlidingExpiration = _cachingOptions.Value.SlidingExpiration,
                    AbsoluteExpirationRelativeToNow = _cachingOptions.Value.AbsoluteExpiration,
                }
            );
    }

    [Fact]
    public async Task SendAsync_DoesNotCache_NonBufferedContent()
    {
        var cache = Substitute.For<IMemoryCache>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream([1, 2, 3])),
        };
        var handler = new MemoryCachingHandler(cache, _cachingOptions)
        {
            InnerHandler = new TestHandler(responseMessage),
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        _ = await invoker.SendAsync(request, CancellationToken.None);

        cache.DidNotReceiveWithAnyArgs().Set<HttpResponseMessage>(default!, default!);
    }

    [Fact]
    public async Task SendAsync_CachedResponse_IsIndependentInstance()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new MemoryCachingHandler(cache, _cachingOptions)
        {
            InnerHandler = new TestHandler(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("test") }
            ),
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        // First call: populates cache
        var firstResponse = await invoker.SendAsync(request, CancellationToken.None);
        // Second call: should hit cache
        var secondResponse = await invoker.SendAsync(request, CancellationToken.None);

        // Mutate the cached response
        secondResponse.Headers.Add(
            "X-Mutated",
            "yes, but this should not affect the cached response"
        );

        // Third call: should get a mutated header (since same instance is returned)
        var thirdResponse = await invoker.SendAsync(request, CancellationToken.None);

        await Verify(thirdResponse, _verifySettings);
        var content = await thirdResponse.Content.ReadAsStringAsync();
        content.ShouldBe("test");
    }

    [Fact]
    public async Task SendAsync_CachedResponse_RequestMessageIsNull()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new MemoryCachingHandler(cache, _cachingOptions)
        {
            InnerHandler = new TestHandler(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("test") }
            ),
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        // Populate cache
        await invoker.SendAsync(request, CancellationToken.None);
        // Retrieve from cache
        var cachedResponse = await invoker.SendAsync(request, CancellationToken.None);

        cachedResponse.RequestMessage.ShouldBeNull();
    }

    // Helper handler to simulate responses
    private class TestHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage _response;

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(_response);
    }
}
