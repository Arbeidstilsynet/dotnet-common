using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnAdapterExtensionTests
{
    private readonly VerifySettings _verifySettings = new();

    //testdata
    private static readonly string App = "test";
    private static readonly string Org = "dat";
    private static readonly string InstanceGuid = "651375fb-4f0f-4857-9f8d-a37d82a3aeb6";
    private static readonly string InstanceOwnerPartyId = "51644866";
    private static readonly string OrganisationNumber = "313546071";
    private static readonly DateTime ProcessStarted = new(2025, 09, 25, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ProcessEnded = new(2025, 09, 25, 13, 0, 0, DateTimeKind.Utc);

    private static readonly AltinnMetadata SampleAltinnMetadata = new AltinnMetadata()
    {
        App = App,
        Org = Org,
        InstanceGuid = Guid.Parse(InstanceGuid),
        InstanceOwnerPartyId = InstanceOwnerPartyId,
        OrganisationNumber = OrganisationNumber,
        ProcessStarted = ProcessStarted,
        ProcessEnded = ProcessEnded,
    };

    public AltinnAdapterExtensionTests()
    {
        _verifySettings.DontScrubDateTimes();
        _verifySettings.DontScrubGuids();
        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    [Fact]
    public async Task AltinnInstance_Maps_ToAltinnMetadata()
    {
        //arrange
        var metadata = new AltinnInstance()
        {
            AppId = $"{Org}/{App}",
            Id = $"{InstanceOwnerPartyId}/{InstanceGuid}",
            InstanceOwner = new InstanceOwner
            {
                OrganisationNumber = OrganisationNumber,
                PartyId = InstanceOwnerPartyId,
            },
            Org = Org,
            Process = new ProcessState { Started = ProcessStarted, Ended = ProcessEnded },
        };
        //act
        var result = metadata.ToAltinnMetadata();
        //assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task AltinnMetadata_Maps_ToInstanceAddress()
    {
        //arrange
        var metadata = SampleAltinnMetadata;
        //act
        var result = metadata.ToInstanceAddress();
        //assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task AltinnInstanceSummary_Maps_ToMetadataDictionary()
    {
        //arrange
        var metadata = new AltinnInstanceSummary
        {
            SkjemaAsPdf = null!,
            Attachments = [],
            Metadata = SampleAltinnMetadata,
        };
        //act
        var result = metadata.ToMetadataDictionary();
        //assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task AltinnMetadata_Maps_ToMetadataDictionary()
    {
        //arrange
        var metadata = SampleAltinnMetadata;
        //act
        var result = metadata.ToMetadataDictionary();
        //assert
        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task Guid_Maps_ToAltinnReference()
    {
        //arrange
        var guid = Guid.Parse(InstanceGuid);
        //act
        var result = guid.ToAltinnReference();
        //assert
        await Verifier.Verify(result, _verifySettings);
    }
}
