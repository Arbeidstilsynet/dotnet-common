using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Unit.Setup;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.ClientTests;

public class DialogportenClientTests : TestBed<AltinnApiTestFixture>
{
    private readonly IAltinnDialogportenClient _sut;

    public DialogportenClientTests(
        ITestOutputHelper testOutputHelper,
        AltinnApiTestFixture altinnApiTestFixture
    )
        : base(testOutputHelper, altinnApiTestFixture)
    {
        _sut = altinnApiTestFixture.GetService<IAltinnDialogportenClient>(testOutputHelper)!;
    }

    [Fact]
    public async Task LookupDialog_WhenCalledWithInstanceRef_ReturnsLookupResponse()
    {
        //act
        var result = await _sut.LookupDialog("some-instance-ref");

        //assert
        result.ShouldNotBeNull();
        result.DialogId.ShouldNotBe(Guid.Empty);
        result.Party.ShouldNotBeNullOrEmpty();
    }
}
