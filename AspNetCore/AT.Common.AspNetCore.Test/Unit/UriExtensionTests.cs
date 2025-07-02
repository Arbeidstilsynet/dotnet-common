using Arbeidstilsynet.Common.AspNetCore.Extensions.Extensions;
using Shouldly;
using Xunit;

namespace AT.Common.AspNetCore.Extensions.Test.Unit;

public class UriExtensionTests
{
    [Theory]
    [InlineData("value with spaces", "value%20with%20spaces")]
    [InlineData("value&with=special?characters", "value%26with%3Dspecial%3Fcharacters")]
    [InlineData("value#with#hash", "value%23with%23hash")]
    [InlineData("value@with@at", "value%40with%40at")]
    [InlineData("value+with+plus", "value%2Bwith%2Bplus")]
    [InlineData("value=with=equals", "value%3Dwith%3Dequals")]
    [InlineData("value%with%percent", "value%25with%25percent")]
    [InlineData("value*with*asterisk", "value%2Awith%2Aasterisk")]
    [InlineData("value,with,comma", "value%2Cwith%2Ccomma")]
    [InlineData("value;with;semicolon", "value%3Bwith%3Bsemicolon")]
    [InlineData("value:with:colon", "value%3Awith%3Acolon")]
    [InlineData("value?with?question", "value%3Fwith%3Fquestion")]
    public void AddQueryParameters_ShouldUrlEncodeValues(string value, string expectedEscapedValue)
    {
        // Arrange
        var uri = new Uri("https://example.com/");

        // Act
        var result = uri.AddQueryParameters(new Dictionary<string, string>() { { "key", value } });

        // Assert
        result.AbsoluteUri.ShouldBe($"https://example.com/?key={expectedEscapedValue}");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void AddQueryParameters_WithEmptyValue_ShouldNotAddParameter(string? emptyValue)
    {
        // Arrange
        var uri = new Uri("https://example.com/");

        // Act
        var result = uri.AddQueryParameters(
            new Dictionary<string, string>()
            {
                { "key", emptyValue! },
                { "anotherKey", "anotherValue" },
            }
        );

        // Assert
        result.AbsoluteUri.ShouldBe("https://example.com/?anotherKey=anotherValue");
    }

    [Fact]
    public void AddQueryParameters_WithNoValidValues_ShouldReturnTheInputUri()
    {
        // Arrange
        var uri = new Uri("https://example.com/");

        // Act
        var result = uri.AddQueryParameters(
            new Dictionary<string, string>() { { "invalid", " " } }
        );

        // Assert
        result.ShouldBe(uri);
    }

    [Fact]
    public void AddQueryParameter_OnAbsoluteUri_ShouldReturnAbsoluteUri()
    {
        // Arrange
        var uri = new Uri("https://example.com/test");

        // Act
        var result = uri.AddQueryParameters(
            new Dictionary<string, string>() { { "key", "value" } }
        );

        // Assert
        result.ShouldNotBe(uri);
        result.AbsoluteUri.ShouldBe("https://example.com/test?key=value");
        result.IsAbsoluteUri.ShouldBeTrue();
    }

    [Fact]
    public void AddQueryParameter_OnRelativeUri_ShouldReturnRelativeUri()
    {
        // Arrange
        var uri = new Uri("test", UriKind.Relative);

        // Act
        var result = uri.AddQueryParameters(
            new Dictionary<string, string>() { { "key", "value" } }
        );

        // Assert
        result.ShouldNotBe(uri);
        result.ToString().ShouldBe("test?key=value");
        result.IsAbsoluteUri.ShouldBeFalse();
    }
}
