using Arbeidstilsynet.Common.Enhetsregisteret.Model.Request;
using Arbeidstilsynet.Common.Enhetsregisteret.Ports;
using Arbeidstilsynet.Common.Enhetsregisteret.Test.Integration.Setup;
using Xunit;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Integration;

public class EnhetsregisteretClientIntegrationTests : TestBed<EnhetsregisteretTestFixture>
{
    private readonly IEnhetsregisteret _sut;
    private readonly VerifySettings _verifySettings = new();
    
    public EnhetsregisteretClientIntegrationTests(ITestOutputHelper testOutputHelper, EnhetsregisteretTestFixture fixture) : base(testOutputHelper, fixture)
    {
        _sut = fixture.GetService<IEnhetsregisteret>(testOutputHelper)!;

        _verifySettings.UseDirectory("TestData/Snapshots");
    }
    
    
    [Fact]
    public async Task GetEnhet_ValidOrganisasjonsnummer_ReturnsEnhet()
    {
        // Arrange
        var organisasjonsnummer = "112233445"; // From OpenApi spec example data

        // Act
        var enhet = await _sut.GetEnhet(organisasjonsnummer);

        // Assert
        await Verify(enhet, _verifySettings);
    }
    
    [Fact]
    public async Task GetUnderenhet_ValidOrganisasjonsnummer_ReturnsUnderenhet()
    {
        // Arrange
        var organisasjonsnummer = "112233445"; // From OpenApi spec example data

        // Act
        var underenhet = await _sut.GetUnderenhet(organisasjonsnummer);

        // Assert
        await Verify(underenhet, _verifySettings);
    }
    
    [Fact]
    public async Task SearchEnheter_ValidRequest_ReturnsEnheter()
    {
        // Act
        var enheter = await _sut.SearchEnheter(new SearchEnheterQuery(), new Pagination());
        
        // Assert
        await Verify(enheter, _verifySettings);
    }
    
    [Fact]
    public async Task GetOppdateringerUnderenheter_ValidRequest_ReturnsOppdateringer()
    {
        // Act
        var oppdateringer = await _sut.GetOppdateringerUnderenheter(new GetOppdateringerQuery()
        {
            Dato = DateTime.Now
        }, new Pagination());
        
        // Assert
        await Verify(oppdateringer, _verifySettings);
    }
    
    [Fact]
    public async Task GetOppdateringerEnheter_ValidRequest_ReturnsOppdateringer()
    {
        // Act
        var oppdateringer = await _sut.GetOppdateringerEnheter(new GetOppdateringerQuery()
        {
            Dato = DateTime.Now
        }, new Pagination());
        
        // Assert
        await Verify(oppdateringer, _verifySettings);
    }

}