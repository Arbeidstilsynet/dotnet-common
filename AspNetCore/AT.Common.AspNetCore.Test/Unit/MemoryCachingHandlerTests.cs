using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Shouldly;
using Xunit;

namespace AT.Common.AspNetCore.Extensions.Test.Unit;

public class MemoryCachingHandlerTests
{
    [Fact]
    public async Task SendAsync_CachesSuccessfulGetResponse()
    {
        // Arrange
        var cache = Substitute.For<IMemoryCache>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("response")
        }; 
        var handler = new MemoryCachingHandler(cache)
        {
            InnerHandler = new TestHandler(responseMessage)
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        // Act
        var response = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        cache.Received(1).Set(request.RequestUri!.ToString(), responseMessage);
    }

    [Fact]
    public async Task SendAsync_DoesNotCache_NonGetRequests()
    {
        var cache = Substitute.For<IMemoryCache>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new MemoryCachingHandler(cache)
        {
            InnerHandler = new TestHandler(responseMessage)
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://test/api");

        await invoker.SendAsync(request, CancellationToken.None);

        cache.DidNotReceiveWithAnyArgs().Set<HttpResponseMessage>(default!, default!);
    }

    [Fact]
    public async Task SendAsync_ReturnsCachedResponse_WhenCacheHit()
    {
        var cache = Substitute.For<IMemoryCache>();
        var cachedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("cached")
        };
        cache.TryGetValue(Arg.Any<object>(), out Arg.Any<object?>())
            .Returns(x => { x[1] = cachedResponse; return true; });

        var handler = new MemoryCachingHandler(cache)
        {
            InnerHandler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("should not be used")
            })
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        var response = await invoker.SendAsync(request, CancellationToken.None);
        
        response.ShouldBe(cachedResponse);
    }

    [Fact]
    public async Task SendAsync_DoesNotCache_UnsuccessfulResponse()
    {
        var cache = Substitute.For<IMemoryCache>();
        var handler = new MemoryCachingHandler(cache)
        {
            InnerHandler = new TestHandler(new HttpResponseMessage(HttpStatusCode.BadRequest))
        };
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://test/api");

        await invoker.SendAsync(request, CancellationToken.None);

        cache.DidNotReceiveWithAnyArgs().Set<HttpResponseMessage>(default!, default!);
    }

    // Helper handler to simulate responses
    private class TestHandler : DelegatingHandler
    {
        private readonly HttpResponseMessage _response;
        public TestHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }
}