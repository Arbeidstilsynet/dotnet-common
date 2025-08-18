using System.Text.Json;
using Arbeidstilsynet.Common.AspNetCore.Extensions.CrossCutting;
using Shouldly;

namespace Arbeidstilsynet.Common.AspNetCore.Extensions.Test.Unit;

public class JsonStringUriConverterTests
{
    private readonly JsonStringUriConverter _sut = new();

    [Theory]
    [InlineData("\"https://example.com\"", "https://example.com/")]
    [InlineData("\"http://example.com/path?query=123\"", "http://example.com/path?query=123")]
    [InlineData("\"ftp://example.com/resource.txt\"", "ftp://example.com/resource.txt")]
    [InlineData("\"https://example.com:8080\"", "https://example.com:8080/")]
    [InlineData("\"https://example.com/path/to/resource\"", "https://example.com/path/to/resource")]
    [InlineData("\"https://example.com/path/to/resource?query=123#fragment\"", "https://example.com/path/to/resource?query=123#fragment")]
    public void Read_ValidUri_ReturnsUri(string url, string expectedUri)
    {
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(url));

        reader.Read(); // Move to the start of the string
        var uri = _sut.Read(ref reader, typeof(Uri), new JsonSerializerOptions());

        uri.ShouldNotBeNull();
        uri.ToString().ShouldBe(expectedUri);
    }

    [Fact]
    public void Read_NotAbsoluteUri_ThrowsJsonException()
    {
        const string json = "\"not-absolute-uri\"";

        var act = Act;

        act.ShouldThrow<JsonException>().Message.ShouldBe("Invalid URI: 'not-absolute-uri'.");

        return;

        void Act()
        {
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            reader.Read(); // Move to the start of the string
            _sut.Read(ref reader, typeof(Uri), new JsonSerializerOptions());
        }
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("null")]
    public void Read_NullEmptyOrWhitespace_ReturnsNull(string json)
    {
        var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));

        reader.Read(); // Move to the null token
        var uri = _sut.Read(ref reader, typeof(Uri), new JsonSerializerOptions());

        uri.ShouldBeNull();
    }

    [Fact]
    public void Write_NullUri_WritesNullValue()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        _sut.Write(writer, null, new JsonSerializerOptions());
        writer.Flush();

        var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        result.ShouldBe("null");
    }

    [Fact]
    public void Write_ValidUri_WritesStringValue()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        var uri = new Uri("https://example.com");
        _sut.Write(writer, uri, new JsonSerializerOptions());
        writer.Flush();

        var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        result.ShouldBe("\"https://example.com/\"");
    }

    [Fact]
    public void Write_EmptyUri_WritesNullValue()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        _sut.Write(writer, new Uri(""), new JsonSerializerOptions());
        writer.Flush();

        var result = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        result.ShouldBe("null");
    }
}
