using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Arbeidstilsynet.Common.EraClient;
using Arbeidstilsynet.Common.EraClient.Extensions;
using Arbeidstilsynet.Common.EraClient.Model;
using Arbeidstilsynet.Common.EraClient.Test.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using NSubstitute.Routing.Handlers;
using Shouldly;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.EraClient.Test;

public class AltinnExtensionTests
{
    private readonly IHostEnvironment _hostEnvironment = Substitute.For<IHostEnvironment>();

    private readonly HttpContext _httpContext = new DefaultHttpContext();

    [Fact]
    public void IsFeatureEnabled_WhenCalledWithExistingTestFlagCookie_ReturnsTrue()
    {
        //arrange
        _hostEnvironment.EnvironmentName.Returns("Development");
        _httpContext.Request.Cookies = MockRequestCookieCollection(
            "TEST_FLAGG",
            "deaktiver_dorvakt&deaktiver_duplikatsjekk&valid"
        );
        //act
        var result = _hostEnvironment.IsFeatureEnabled("valid", _httpContext);
        //assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsFeatureEnabled_WhenCalledWithIncompleteTestFlagCookie_ReturnsFalse()
    {
        //arrage
        _hostEnvironment.EnvironmentName.Returns("Development");
        _httpContext.Request.Cookies = MockRequestCookieCollection(
            "TEST_FLAGG",
            "deaktiver_dorvakt&deaktiver_duplikatsjekk"
        );
        //act
        var result = _hostEnvironment.IsFeatureEnabled("valid", _httpContext);
        //assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsFeatureEnabled_WhenCalledWithoutCookie_ReturnsFalse()
    {
        //arrage
        _hostEnvironment.EnvironmentName.Returns("Development");
        _httpContext.Request.Cookies = MockRequestCookieCollection("", "");
        //act
        var result = _hostEnvironment.IsFeatureEnabled("valid", _httpContext);
        //assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GetRespectiveEraEnvironment_WhenCalledWithoutCookies_ReturnsVerifi()
    {
        //arrange
        _hostEnvironment.EnvironmentName.Returns("Development");
        _httpContext.Request.Cookies = MockRequestCookieCollection("", "");
        //act
        var result = _hostEnvironment.GetRespectiveEraEnvironment(_httpContext).MapToString();
        //assert
        result.ShouldBeEquivalentTo("verifi");
    }

    [Fact]
    public void GetRespectiveEraEnvironment_WhenCalledWithProductionEnvironment_ReturnsProd()
    {
        //arrange
        _hostEnvironment.EnvironmentName.Returns("Production");
        _httpContext.Request.Cookies = MockRequestCookieCollection(
            "TEST_FLAGG",
            "deaktiver_dorvakt&deaktiver_duplikatsjekk&valid"
        );
        //act
        var result = _hostEnvironment.GetRespectiveEraEnvironment(_httpContext).MapToString();
        //assert
        result.ShouldBeEquivalentTo("prod");
    }

    [Fact]
    public void GetRespectiveEraEnvironment_WhenCalledWithValidCookie_ReturnsValid()
    {
        //arrange
        _hostEnvironment.EnvironmentName.Returns("Development");
        _httpContext.Request.Cookies = MockRequestCookieCollection(
            "TEST_FLAGG",
            "deaktiver_dorvakt&deaktiver_duplikatsjekk&valid"
        );
        //act
        var result = _hostEnvironment.GetRespectiveEraEnvironment(_httpContext).MapToString();
        //assert
        result.ShouldBeEquivalentTo("valid");
    }

    private static IRequestCookieCollection MockRequestCookieCollection(string key, string value)
    {
        var requestFeature = new HttpRequestFeature();
        var featureCollection = new FeatureCollection();

        requestFeature.Headers = new HeaderDictionary();
        requestFeature.Headers.Append(HeaderNames.Cookie, new StringValues(key + "=" + value));

        featureCollection.Set<IHttpRequestFeature>(requestFeature);

        var cookiesFeature = new RequestCookiesFeature(featureCollection);

        return cookiesFeature.Cookies;
    }
}
