using System.Net;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Test.Unit.TestData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class AltinnAdapterTests
{
    private readonly IAltinnStorageClient _storageClient;
    private readonly IAltinnEventsClient _eventsClient;
    private readonly IOptions<AltinnConfiguration> _configuration;
    private readonly ILogger<AltinnAdapter> _logger;
    private readonly AltinnAdapter _adapter;

    public AltinnAdapterTests()
    {
        _storageClient = Substitute.For<IAltinnStorageClient>();
        _eventsClient = Substitute.For<IAltinnEventsClient>();
        _logger = Substitute.For<ILogger<AltinnAdapter>>();

        _configuration = Options.Create(
            new AltinnConfiguration
            {
                OrgId = "dat",
                AuthenticationUrl = new Uri("https://platform.altinn.no/authentication/api/v1"),
                StorageUrl = new Uri("https://platform.altinn.no/storage/api/v1"),
                EventUrl = new Uri("https://platform.altinn.no/events/api/v1"),
                AppBaseUrl = new Uri("https://dat.apps.altinn.no"),
            }
        );

        _adapter = new AltinnAdapter(_storageClient, _eventsClient, _configuration, _logger);
    }

    #region GetSummary Tests

    [Fact]
    public async Task GetSummary_ReturnsCorrectSummary_ForInstanceWithMainPdfOnly()
    {
        // Arrange
        var cloudEvent = AltinnTestData.CreateAltinnCloudEvent();
        var instance = AltinnTestData.CreateAltinnInstance(
            dataElements: AltinnTestData.CreateDefaultDataElements()
        );

        _storageClient.GetInstance(cloudEvent).Returns(instance);
        _storageClient
            .GetInstanceData(Arg.Any<InstanceDataRequest>())
            .Returns(new MemoryStream([1, 2, 3]));

        // Act
        var summary = await _adapter.GetSummary(cloudEvent);

        // Assert
        summary.ShouldNotBeNull();
        summary.Metadata.ShouldNotBeNull();
        summary.SkjemaAsPdf.ShouldNotBeNull();
        summary.StructuredData.ShouldBeNull();
        summary.Attachments.ShouldBeEmpty();

        await _storageClient.Received(1).GetInstance(cloudEvent);
        await _storageClient.Received(1).GetInstanceData(Arg.Any<InstanceDataRequest>());
    }

    [Fact]
    public async Task GetSummary_ReturnsCorrectSummary_ForInstanceWithStructuredDataAndAttachments()
    {
        // Arrange
        var cloudEvent = AltinnTestData.CreateAltinnCloudEvent();
        var instance = AltinnTestData.CreateAltinnInstance(
            dataElements: AltinnTestData.CreateDefaultDataElements(
                structuredDataTypeId: "model",
                attachmentCount: 2
            ),
            dataValues: new Dictionary<string, string> { { "StructuredDataTypeId", "model" } }
        );

        _storageClient.GetInstance(cloudEvent).Returns(instance);
        _storageClient
            .GetInstanceData(Arg.Any<InstanceDataRequest>())
            .Returns(callInfo => new MemoryStream([1, 2, 3]));

        // Act
        var summary = await _adapter.GetSummary(cloudEvent);

        // Assert
        summary.ShouldNotBeNull();
        summary.Metadata.ShouldNotBeNull();
        summary.SkjemaAsPdf.ShouldNotBeNull();
        summary.StructuredData.ShouldNotBeNull();
        summary.Attachments.Count.ShouldBe(2);

        await _storageClient.Received(1).GetInstance(cloudEvent);
        // 1 for main PDF + 1 for structured data + 2 for attachments = 4 calls
        await _storageClient.Received(4).GetInstanceData(Arg.Any<InstanceDataRequest>());
    }

    [Fact]
    public async Task GetSummary_PopulatesMetadataCorrectly()
    {
        // Arrange
        var partyId = "12345";
        var instanceGuid = Guid.NewGuid();
        var cloudEvent = AltinnTestData.CreateAltinnCloudEvent(partyId: partyId);
        var instance = AltinnTestData.CreateAltinnInstance(
            id: instanceGuid,
            appId: "dat/test-app",
            partyId: partyId,
            organisationNumber: "987654321"
        );

        _storageClient.GetInstance(cloudEvent).Returns(instance);
        _storageClient
            .GetInstanceData(Arg.Any<InstanceDataRequest>())
            .Returns(new MemoryStream([1, 2, 3]));

        // Act
        var summary = await _adapter.GetSummary(cloudEvent);

        // Assert
        summary.Metadata.InstanceGuid.ShouldBe(instanceGuid);
        summary.Metadata.InstanceOwnerPartyId.ShouldBe(partyId);
        summary.Metadata.Org.ShouldBe("dat");
        summary.Metadata.App.ShouldBe("test-app");
        summary.Metadata.OrganisationNumber.ShouldBe("987654321");
    }

    #endregion

    #region SubscribeForCompletedProcessEvents Tests

    [Fact]
    public async Task SubscribeForCompletedProcessEvents_CreatesCorrectSubscriptionRequest()
    {
        // Arrange
        var callbackUrl = new Uri("https://example.com/callback");
        var appId = "test-app";
        var subscriptionRequest = AltinnTestData.CreateSubscriptionRequestDto(callbackUrl, appId);
        var expectedSubscription = AltinnTestData.CreateAltinnSubscription();

        _eventsClient.Subscribe(Arg.Any<AltinnSubscriptionRequest>()).Returns(expectedSubscription);

        // Act
        var result = await _adapter.SubscribeForCompletedProcessEvents(subscriptionRequest);

        // Assert
        result.ShouldBe(expectedSubscription);
        await _eventsClient
            .Received(1)
            .Subscribe(
                Arg.Is<AltinnSubscriptionRequest>(req =>
                    req.EndPoint == callbackUrl
                    && req.TypeFilter == "app.instance.process.completed"
                    && req.SourceFilter.ToString() == "https://dat.apps.altinn.no/dat/test-app"
                )
            );
    }

    [Fact]
    public async Task SubscribeForCompletedProcessEvents_UsesConfiguredOrgId()
    {
        // Arrange
        var subscriptionRequest = AltinnTestData.CreateSubscriptionRequestDto(
            altinnAppId: "my-app"
        );
        var expectedSubscription = AltinnTestData.CreateAltinnSubscription();

        _eventsClient.Subscribe(Arg.Any<AltinnSubscriptionRequest>()).Returns(expectedSubscription);

        // Act
        await _adapter.SubscribeForCompletedProcessEvents(subscriptionRequest);

        // Assert
        await _eventsClient
            .Received(1)
            .Subscribe(
                Arg.Is<AltinnSubscriptionRequest>(req =>
                    req.SourceFilter.ToString().Contains("dat/my-app")
                )
            );
    }

    #endregion

    #region UnsubscribeForCompletedProcessEvents Tests

    [Fact]
    public async Task UnsubscribeForCompletedProcessEvents_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var subscription = AltinnTestData.CreateAltinnSubscription(id: 123);
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        _eventsClient.Unsubscribe(123).Returns(response);

        // Act
        var result = await _adapter.UnsubscribeForCompletedProcessEvents(subscription);

        // Assert
        result.ShouldBeTrue();
        await _eventsClient.Received(1).Unsubscribe(123);
    }

    [Fact]
    public async Task UnsubscribeForCompletedProcessEvents_ReturnsFalse_WhenNotSuccessful()
    {
        // Arrange
        var subscription = AltinnTestData.CreateAltinnSubscription(id: 123);
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _eventsClient.Unsubscribe(123).Returns(response);

        // Act
        var result = await _adapter.UnsubscribeForCompletedProcessEvents(subscription);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region GetMetadataForNonCompletedInstances Tests

    [Fact]
    public async Task GetMetadataForNonCompletedInstances_ReturnsMetadataForAllInstances()
    {
        // Arrange
        var appId = "test-app";
        var instances = new List<AltinnInstance>
        {
            AltinnTestData.CreateAltinnInstance(appId: "dat/test-app"),
            AltinnTestData.CreateAltinnInstance(appId: "dat/test-app"),
            AltinnTestData.CreateAltinnInstance(appId: "dat/test-app"),
        };
        var response = new AltinnQueryResponse<AltinnInstance>()
        {
            Instances = instances,
            Count = instances.Count,
        };

        _storageClient.GetInstances(default!).ReturnsForAnyArgs(response);

        // Act
        var result = await _adapter.GetMetadataForNonCompletedInstances(appId);

        // Assert
        var metadataList = result.ToList();
        metadataList.Count.ShouldBe(3);
        metadataList[0].InstanceGuid.ShouldBe(instances[0].GetInstanceGuid());
        metadataList[1].InstanceGuid.ShouldBe(instances[1].GetInstanceGuid());
        metadataList[2].InstanceGuid.ShouldBe(instances[2].GetInstanceGuid());
    }

    [Fact]
    public async Task GetMetadataForNonCompletedInstances_UsesCorrectQueryParameters()
    {
        // Arrange
        var appId = "test-app";

        _storageClient
            .GetInstances(default!)
            .ReturnsForAnyArgs(new AltinnQueryResponse<AltinnInstance>());

        // Act
        await _adapter.GetMetadataForNonCompletedInstances(appId, processIsComplete: false);

        // Assert
        await _storageClient
            .Received(1)
            .GetInstances(
                Arg.Is<InstanceQueryParameters>(p =>
                    p.AppId == "dat/test-app"
                    && p.Org == "dat"
                    && p.ProcessIsComplete == false
                    && p.ExcludeConfirmedBy == "dat"
                )
            );
    }

    #endregion

    #region GetNonCompletedInstances Tests

    [Fact]
    public async Task GetNonCompletedInstances_ReturnsSummariesForAllInstances()
    {
        // Arrange
        var appId = "test-app";
        var instances = new List<AltinnInstance>
        {
            AltinnTestData.CreateAltinnInstance(),
            AltinnTestData.CreateAltinnInstance(),
        };

        _storageClient
            .GetInstances(Arg.Any<InstanceQueryParameters>())
            .Returns(
                new AltinnQueryResponse<AltinnInstance>
                {
                    Instances = instances,
                    Count = instances.Count,
                }
            );
        _storageClient
            .GetInstanceData(Arg.Any<InstanceDataRequest>())
            .Returns(new MemoryStream([1, 2, 3]));

        // Act
        var result = await _adapter.GetNonCompletedInstances(appId);

        // Assert
        var summaries = result.ToList();
        summaries.Count.ShouldBe(2);
        summaries.All(s => s.SkjemaAsPdf != null).ShouldBeTrue();
    }

    [Fact]
    public async Task GetNonCompletedInstances_RetrievesDataForEachInstance()
    {
        // Arrange
        var appId = "test-app";
        var instances = new List<AltinnInstance>
        {
            AltinnTestData.CreateAltinnInstance(),
            AltinnTestData.CreateAltinnInstance(),
        };

        _storageClient
            .GetInstances(Arg.Any<InstanceQueryParameters>())
            .Returns(
                new AltinnQueryResponse<AltinnInstance>
                {
                    Instances = instances,
                    Count = instances.Count,
                }
            );
        _storageClient
            .GetInstanceData(Arg.Any<InstanceDataRequest>())
            .Returns(new MemoryStream([1, 2, 3]));

        // Act
        await _adapter.GetNonCompletedInstances(appId);

        // Assert
        // 2 instances, each with 1 main PDF = 2 calls
        await _storageClient.Received(2).GetInstanceData(Arg.Any<InstanceDataRequest>());
    }

    #endregion

    #region GetAltinnSubscription Tests

    [Fact]
    public async Task GetAltinnSubscription_ReturnsSubscription_WhenExists()
    {
        // Arrange
        var subscriptionId = 123;
        var expectedSubscription = AltinnTestData.CreateAltinnSubscription(id: subscriptionId);

        _eventsClient.GetAltinnSubscription(subscriptionId).Returns(expectedSubscription);

        // Act
        var result = await _adapter.GetAltinnSubscription(subscriptionId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(subscriptionId);
    }

    [Fact]
    public async Task GetAltinnSubscription_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var subscriptionId = 123;

        _eventsClient
            .GetAltinnSubscription(subscriptionId)
            .Returns<AltinnSubscription>(x =>
                throw new HttpRequestException("Not found", null, HttpStatusCode.NotFound)
            );

        // Act
        var result = await _adapter.GetAltinnSubscription(subscriptionId);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAltinnSubscription_ThrowsException_WhenOtherHttpError()
    {
        // Arrange
        var subscriptionId = 123;

        _eventsClient
            .GetAltinnSubscription(subscriptionId)
            .Returns<AltinnSubscription>(x =>
                throw new HttpRequestException(
                    "Server error",
                    null,
                    HttpStatusCode.InternalServerError
                )
            );

        // Act & Assert
        await Should.ThrowAsync<HttpRequestException>(async () =>
            await _adapter.GetAltinnSubscription(subscriptionId)
        );
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task GetSummary_ThrowsException_WhenInstanceHasNoMainPdf()
    {
        // Arrange
        var cloudEvent = AltinnTestData.CreateAltinnCloudEvent();
        var instance = AltinnTestData.CreateAltinnInstance(
            dataElements: new List<DataElement>
            {
                AltinnTestData.CreateDataElement("not-a-pdf", "application/json"),
            }
        );

        _storageClient.GetInstance(cloudEvent).Returns(instance);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _adapter.GetSummary(cloudEvent)
        );
    }

    [Fact]
    public async Task GetSummary_ThrowsException_WhenDataElementIdIsNull()
    {
        // Arrange
        var cloudEvent = AltinnTestData.CreateAltinnCloudEvent();
        var instance = AltinnTestData.CreateAltinnInstance();
        instance.Data[0].Id = null;

        _storageClient.GetInstance(cloudEvent).Returns(instance);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _adapter.GetSummary(cloudEvent)
        );
    }

    #endregion
}
