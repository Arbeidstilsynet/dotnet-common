using System.Text;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Unit.Setup;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.ClientTests;

public class AltinnCorrespondenceClientTests : TestBed<AltinnApiTestFixture>
{
    private readonly IAltinnCorrespondenceClient _sut;

    public AltinnCorrespondenceClientTests(
        ITestOutputHelper testOutputHelper,
        AltinnApiTestFixture altinnApiTestFixture
    )
        : base(testOutputHelper, altinnApiTestFixture)
    {
        _sut = altinnApiTestFixture.GetService<IAltinnCorrespondenceClient>(testOutputHelper)!;
    }

    [Fact]
    public async Task InitializeCorrespondence_WhenCalledWithoutAttachments_ReturnsExampleResponse()
    {
        //arrange
        CorrespondenceRequest request = new()
        {
            ResourceId = "urn:altinn:resource:your-resource-id",
            SendersReference = "a-unique-string",
            Recipients = ["urn:altinn:organization:identifier-no:123123123"],
            Content = new() { MessageTitle = "Test", MessageBody = "This is a test" },
        };
        //act
        var result = await _sut.InitializeCorrespondence(request, null);
        //assert
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task InitializeCorrespondence_WhenCalledWithAttachments_ReturnsExampleResponse()
    {
        //arrange
        var attachment1 = Substitute.For<IFormFile>();
        attachment1.FileName.Returns("file1");
        attachment1.ContentType.Returns("application/txt");
        attachment1
            .OpenReadStream()
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes("first file content")));
        var attachment2 = Substitute.For<IFormFile>();
        attachment2.FileName.Returns("file2");
        attachment2
            .OpenReadStream()
            .Returns(new MemoryStream(Encoding.UTF8.GetBytes("second file content")));
        attachment2.ContentType.Returns("application/txt");
        CorrespondenceRequest request = new()
        {
            ResourceId = "urn:altinn:resource:your-resource-id",
            SendersReference = "a-unique-string",
            Recipients = ["urn:altinn:organization:identifier-no:123123123"],
            Content = new()
            {
                MessageTitle = "Test",
                MessageBody = "This is a test",
                Attachments =
                [
                    new() { FileName = "file1", SendersReference = "first-attachment" },
                    new() { FileName = "file2", SendersReference = "second-attachment" },
                ],
            },
        };
        //act
        var result = await _sut.InitializeCorrespondence(request, [attachment1, attachment2]);
        //assert
        result.ShouldNotBeNull();
    }
}
