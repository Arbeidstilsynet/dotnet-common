using System.Runtime.InteropServices;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnAppSpecificationTests
{
    private readonly AltinnAppSpecification _defaultSpec = new("default");

    private readonly VerifySettings _verifySettings = new();

    public AltinnAppSpecificationTests()
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
        Action act = () => _ = new AltinnAppSpecification(invalidAppId);
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

    [Fact]
    public void GetDataElementsBySignificance_WhenMainPdfIsMissing_Throws()
    {
        var instance = CreateInstance([]);
        Action act = () => _ = instance.GetDataElementsBySignificance();

        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public async Task GetDataElementsBySignificance_WhenCompliant_DoesNotThrow()
    {
        var compliantInstance = CreateCompliantInstance(_defaultSpec);

        var (mainData, _, _) = compliantInstance.GetDataElementsBySignificance();

        await Verify(mainData, _verifySettings);
    }

    [Fact]
    public void GetDataElementsBySignificance_WhenFull_DoesNotThrow()
    {
        var compliantInstance = CreateCompliantInstance(
            _defaultSpec,
            [
                CreateDataElement(_defaultSpec.StructuredDataTypeId, "application/json"),
                CreateDataElement("attachment1.pdf", "application/pdf"),
                CreateDataElement("attachment2.xml", "application/xml"),
            ]
        );

        var (mainData, structuredData, attachmentData) =
            compliantInstance.GetDataElementsBySignificance();

        mainData.DataType.ShouldBe(_defaultSpec.MainPdfDataTypeId);
        structuredData.ShouldNotBeNull();
        structuredData.DataType.ShouldBe(_defaultSpec.StructuredDataTypeId);
        attachmentData.ToList().Count.ShouldBe(2);
    }

    [Theory]
    [InlineData("attachment-type-1", "application/pdf")]
    [InlineData("structured-data", "application/json")]
    [InlineData("ref-data-as-pdf", "application/pdf")]
    public async Task CreateFileMetadata_AttachmentDataElement_CreatesExpectedMetadata(
        string dataType,
        string contentType
    )
    {
        var dataElement = CreateDataElement(dataType, contentType);

        var fileMetadata = _defaultSpec.CreateFileMetadata(dataElement);

        await Verify(fileMetadata, _verifySettings).UseParameters(dataType, contentType);
    }

    private static AltinnInstance CreateCompliantInstance(
        AltinnAppSpecification spec,
        params DataElement[] additionalDataElements
    )
    {
        return CreateInstance(
            new Dictionary<string, string>()
            {
                {
                    AltinnSpecificationExtensions.StructuredDataTypeIdKey,
                    spec.StructuredDataTypeId
                },
                { AltinnSpecificationExtensions.MainPdfDataTypeId, spec.MainPdfDataTypeId },
            },
            [
                CreateDataElement(spec.MainPdfDataTypeId, "application/pdf"),
                .. additionalDataElements,
            ]
        );
    }

    private static AltinnInstance CreateInstance(
        Dictionary<string, string> dataValues,
        params List<DataElement> dataElements
    )
    {
        return new AltinnInstance()
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111").ToString(),
            AppId = "some-app-id",
            Data = dataElements.ToList(),
            DataValues = dataValues,
        };
    }

    private static DataElement CreateDataElement(
        string dataType,
        string contentType = "application/json"
    )
    {
        return new DataElement()
        {
            Id = Guid.NewGuid().ToString(),
            Filename = "some-filename",
            DataType = dataType,
            ContentType = contentType,
            FileScanResult = FileScanResult.Clean,
        };
    }
}
