using Arbeidstilsynet.Common.AltinnApp.DependencyInjection;
using Arbeidstilsynet.Common.AltinnApp.Implementation;
using Arbeidstilsynet.Common.AltinnApp.Model;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.AltinnApp.Test.Unit;

public class LandOptionsTests
{
    private readonly IOptions<LandOptionsConfiguration> _configuration = Substitute.For<
        IOptions<LandOptionsConfiguration>
    >();

    private readonly ILandskodeLookup _landskodeLookup = Substitute.For<ILandskodeLookup>();

    public LandOptionsTests()
    {
        _configuration.Value.Returns(new LandOptionsConfiguration());

        _landskodeLookup.ClearSubstitute();
    }

    [Fact]
    public void LandOptionsConfiguration_DefaultOptionsIdShouldBe_land()
    {
        new LandOptionsConfiguration().OptionsId.ShouldBe("land");
    }

    [Fact]
    public void Constructor_ShouldInitializeWithOptionsIdFromConfiguration()
    {
        // Arrange
        var expectedId = "customOptionsId";
        _configuration.Value.Returns(new LandOptionsConfiguration { OptionsId = expectedId });

        // Act
        var sut = new LandOptions(_landskodeLookup, _configuration);

        // Assert
        sut.Id.ShouldBe(expectedId);
    }

    [Fact]
    public async Task GetAppOptionsAsync_ShouldReturnAllLandskoder()
    {
        // Arrange
        var expectedLandskoder = new List<KeyValuePair<string, Landskode>>
        {
            new("NOR", new Landskode("Norway", "+47", "NO", "NOR")),
            new("SWE", new Landskode("Sweden", "+46", "SE", "SWE")),
            new("FIN", new Landskode("Finland", "+358", "FI", "FIN")),
        };

        _landskodeLookup.GetLandskoder().Returns(expectedLandskoder);

        var sut = new LandOptions(_landskodeLookup, _configuration);

        // Act
        var result = await sut.GetAppOptionsAsync(default, default!);

        // Assert
        result.Options.ShouldNotBeNull();
        result.Options.Count.ShouldBe(expectedLandskoder.Count);
        result.Options[0].Label.ShouldBe("Norway");
        result.Options[0].Value.ShouldBe("NOR");
    }
    
    [Fact]
    public async Task GetAppOptionsAsync_ShouldUseCustomOrder()
    {
        // Arrange
        var expectedLandskoder = new List<KeyValuePair<string, Landskode>>
        {
            new("NOR", new Landskode("Norway", "+47", "NO", "NOR")),
            new("SWE", new Landskode("Sweden", "+46", "SE", "SWE")),
            new("FIN", new Landskode("Finland", "+358", "FI", "FIN")),
        };

        _landskodeLookup.GetLandskoder().Returns(expectedLandskoder);

        _configuration.Value.Returns(new LandOptionsConfiguration()
        {
            CustomOrderFunc = lands => lands.Reverse()
        });
        
        var sut = new LandOptions(_landskodeLookup, _configuration);

        // Act
        var result = await sut.GetAppOptionsAsync(default, default!);

        // Assert
        result.Options.ShouldNotBeNull();
        result.Options.Count.ShouldBe(expectedLandskoder.Count);
        result.Options[0].Label.ShouldBe("Finland");
        result.Options[1].Label.ShouldBe("Sweden");
        result.Options[2].Label.ShouldBe("Norway");
    }
    
    [Theory]
    [InlineData(LandOptionsConfiguration.IsoType.Alpha2, "NO")]
    [InlineData(LandOptionsConfiguration.IsoType.Alpha3, "NOR")]
    public async Task GetAppOptionsAsync_ShouldUseConfiguredIsoType(
        LandOptionsConfiguration.IsoType isoType,
        string expectedValue
    )
    {
        // Arrange
        var expectedLandskoder = new List<KeyValuePair<string, Landskode>>
        {
            new("NOR", new Landskode("Norway", "+47", "NO", "NOR"))
        };

        _landskodeLookup.GetLandskoder().Returns(expectedLandskoder);

        _configuration.Value.Returns(new LandOptionsConfiguration()
        {
            OptionValueIsoType = isoType
        });
        
        var sut = new LandOptions(_landskodeLookup, _configuration);

        // Act
        var result = await sut.GetAppOptionsAsync(default, default!);

        // Assert
        result.Options.ShouldNotBeNull();
        result.Options.Count.ShouldBe(expectedLandskoder.Count);
        result.Options[0].Label.ShouldBe("Norway");
        result.Options[0].Value.ShouldBe(expectedValue);
    }
}
