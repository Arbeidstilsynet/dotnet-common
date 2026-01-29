using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnSpecificationTests
{
    private readonly AltinnAppSpecification _defaultSpec = new("default");

    private readonly VerifySettings _verifySettings = new();

    public AltinnSpecificationTests()
    {
        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    [Fact]
    public async Task DefaultsAreCorrect()
    {
        await Verify(_defaultSpec, _verifySettings);
    }

    [Theory]
    [InlineData("")]
    [InlineData("orgButNoApp/")]
    [InlineData("/")]
    public void Ctor_Throws_ForInvalidAppId(string invalidAppId)
    {
        var act = () => new AltinnAppSpecification(invalidAppId);
        act.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData("org/app", "app")]
    [InlineData("app", "app")]
    [InlineData("/app", "app")]
    public void Ctor_Succeeds_ForValidAppId(string validAppId, string expectedAppId)
    {
        var spec = new AltinnAppSpecification(validAppId);
        spec.AppId.ShouldBe(expectedAppId);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("orgButNoApp/", null)]
    [InlineData("/", null)]
    [InlineData("org/app", "app")]
    [InlineData("app", "app")]
    [InlineData("/app", "app")]
    public void SanitizeAppId_RetrunsExpectedResult(string? appId, string? expectedResult)
    {
        appId.SanitizeAppId().ShouldBe(expectedResult);
    }
}
