using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Implementation;
using Arbeidstilsynet.Common.Altinn.Model;
using Arbeidstilsynet.Common.Altinn.Ports;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Shouldly;
using Xunit;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class LandOptionsTests
{
    private readonly ILandskodeLookup _landskodeLookup = Substitute.For<ILandskodeLookup>();
    private readonly IOptions<LandOptionsConfiguration> _configuration = Substitute.For<
        IOptions<LandOptionsConfiguration>
    >();

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
        _configuration.Value.Returns(new LandOptionsConfiguration() { OptionsId = expectedId });

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
            new("NOR", new Landskode("Norway", "+47")),
            new("SWE", new Landskode("Sweden", "+46")),
            new("FIN", new Landskode("Finland", "+358")),
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
}
