using Arbeidstilsynet.Common.Enhetsregisteret.Implementation;
using Arbeidstilsynet.Common.Enhetsregisteret.Models;
using Shouldly;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class PaginatedResponseExtensionsTests
{
    [Theory]
    [InlineData(1, 4, 4)] // exact multiples
    [InlineData(2, 4, 2)]
    [InlineData(2, 5, 3)] // partial final page rounds up
    [InlineData(3, 2, 1)] // fewer elements than a page
    [InlineData(1000, 0, 0)] // no elements
    public void ToPaginatedResponse_ComputesTotalPages(
        long pageSize,
        long totalElements,
        long expectedTotalPages
    )
    {
        // Arrange
        var response = new Enheter
        {
            Embedded = new Enheter_embedded { Enheter = [new Enhet()] },
            Page = new Page { Size = pageSize, TotalElements = totalElements },
        };

        // Act
        var result = response.ToPaginatedResponse();

        // Assert
        result.TotalPages.ShouldBe(expectedTotalPages);
    }

    [Fact]
    public void ToPaginatedResponse_PageSizeZero_ReturnsZeroTotalPages()
    {
        // Arrange
        var response = new Enheter
        {
            Page = new Page { Size = 0, TotalElements = 42 },
        };

        // Act
        var result = response.ToPaginatedResponse();

        // Assert
        result.TotalPages.ShouldBe(0);
    }

    [Fact]
    public void ToPaginatedResponse_NullPage_ReturnsZeroTotalPages()
    {
        // Arrange
        var response = new Enheter { Page = null };

        // Act
        var result = response.ToPaginatedResponse();

        // Assert
        result.TotalPages.ShouldBe(0);
    }

    [Fact]
    public void ToPaginatedResponse_NullEmbedded_ReturnsEmptyElements()
    {
        // Arrange
        var response = new Enheter
        {
            Embedded = null,
            Page = new Page { Size = 10, TotalElements = 0 },
        };

        // Act
        var result = response.ToPaginatedResponse();

        // Assert
        result.Elements.ShouldBeEmpty();
    }

    [Fact]
    public void ToPaginatedResponse_ExposesEmbeddedElements()
    {
        // Arrange
        var enheter = new List<Enhet> { new(), new() };
        var response = new Enheter
        {
            Embedded = new Enheter_embedded { Enheter = enheter },
            Page = new Page { Size = 10, TotalElements = 2 },
        };

        // Act
        var result = response.ToPaginatedResponse();

        // Assert
        result.Elements.ShouldBe(enheter);
    }

    [Fact]
    public void ToPaginatedResponse_Underenheter_MapsEmbeddedAndPage()
    {
        // Arrange
        var underenheter = new List<Underenhet> { new() };
        var response = new Underenheter
        {
            Embedded = new Underenheter_embedded { Underenheter = underenheter },
            Page = new Page { Size = 1, TotalElements = 3 },
        };

        // Act
        var result = response.ToPaginatedResponse();

        // Assert
        result.Elements.ShouldBe(underenheter);
        result.TotalPages.ShouldBe(3);
    }

    [Fact]
    public void ToPaginatedResponse_OppdateringerEnheter_MapsEmbeddedAndPage()
    {
        // Arrange
        var oppdateringer = new List<OppdateringerEnhet> { new(), new() };
        var response = new OppdateringerEnheter
        {
            Embedded = new OppdateringerEnheter_embedded { OppdaterteEnheter = oppdateringer },
            Page = new Page { Size = 2, TotalElements = 3 },
        };

        // Act
        var result = response.ToPaginatedResponse();

        // Assert
        result.Elements.ShouldBe(oppdateringer);
        result.TotalPages.ShouldBe(2);
    }

    [Fact]
    public void ToPaginatedResponse_OppdateringerUnderenheter_MapsEmbeddedAndPage()
    {
        // Arrange
        var oppdateringer = new List<OppdateringerUnderenhet> { new() };
        var response = new OppdateringerUnderenheter
        {
            Embedded = new OppdateringerUnderenheter_embedded
            {
                OppdaterteUnderenheter = oppdateringer,
            },
            Page = new Page { Size = 5, TotalElements = 5 },
        };

        // Act
        var result = response.ToPaginatedResponse();

        // Assert
        result.Elements.ShouldBe(oppdateringer);
        result.TotalPages.ShouldBe(1);
    }
}
