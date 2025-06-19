using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Arbeidstilsynet.Common.Enhetsregisteret.Test.Integration.Setup;
using Xunit;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Integration;

public class EnhetsregisteretClientIntegrationTests : TestBed<EnhetsregisteretTestFixture>
{
    private readonly IEnhetsregisteret _sut;
    
    public EnhetsregisteretClientIntegrationTests(ITestOutputHelper testOutputHelper, EnhetsregisteretTestFixture fixture) : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IEnhetsregisteret>(testOutputHelper)!;
    }
    
    
    [Fact]
    public async Task GetEnhet_ValidOrganisasjonsnummer_ReturnsEnhet()
    {
        // Arrange
        var organisasjonsnummer = "112233445";

        // Act
        var enhet = await _sut.GetEnhet(organisasjonsnummer);

        // Assert
        Assert.NotNull(enhet); // Replace with Verify
    }

}