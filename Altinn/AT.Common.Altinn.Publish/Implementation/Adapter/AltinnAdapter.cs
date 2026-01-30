using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DataElement = Arbeidstilsynet.Common.Altinn.Model.Api.Response.DataElement;
using FileScanResult = Arbeidstilsynet.Common.Altinn.Model.Api.Response.FileScanResult;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnAdapter(
    IAltinnStorageClient altinnStorageClient,
    IAltinnEventsClient altinnEventsClient,
    IOptions<AltinnConfiguration> altinnApiConfigurationOptions,
    ILogger<AltinnAdapter> logger
) : IAltinnAdapter
{
    public async Task<AltinnInstanceSummary> GetSummary(
        AltinnCloudEvent cloudEvent,
        AltinnAppSpecification appSpec
    )
    {
        // get instans basert på cloud event data
        var instance = await altinnStorageClient.GetInstance(cloudEvent);

        return await GetInstanceSummaryAsync(instance, appSpec);
    }

    public Task<AltinnInstanceSummary> GetSummary(AltinnCloudEvent cloudEvent, string appId)
    {
        return GetSummary(cloudEvent, new AltinnAppSpecification(appId));
    }

    public Task<AltinnSubscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto
    )
    {
        var baseUrl = altinnApiConfigurationOptions.Value.AppBaseUrl;
        var orgId = altinnApiConfigurationOptions.Value.OrgId;
        var appId = subscriptionRequestDto.AltinnAppId;

        var mappedRequest = new AltinnSubscriptionRequest()
        {
            SourceFilter = new Uri(baseUrl, $"{orgId}/{appId}"),
            EndPoint = subscriptionRequestDto.CallbackUrl,
            TypeFilter = "app.instance.process.completed",
        };
        logger.LogInformation(
            "Sending subscription request with the following options: {SourceFilter}, {Endpoint}, {TypeFilter}",
            mappedRequest.SourceFilter,
            mappedRequest.EndPoint,
            mappedRequest.TypeFilter
        );
        return altinnEventsClient.Subscribe(mappedRequest);
    }

    public async Task<bool> UnsubscribeForCompletedProcessEvents(
        AltinnSubscription altinnSubscription
    )
    {
        var response = await altinnEventsClient.Unsubscribe(altinnSubscription.Id);
        return response.StatusCode == System.Net.HttpStatusCode.OK;
    }

    public async Task<IEnumerable<AltinnInstanceSummary>> GetNonCompletedInstances(
        AltinnAppSpecification appSpec,
        bool processIsComplete = true
    )
    {
        var orgId = altinnApiConfigurationOptions.Value.OrgId;
        var appId = appSpec.AppId;

        var instances = await altinnStorageClient.GetAllInstances(
            new InstanceQueryParameters
            {
                AppId = $"{orgId}/{appId}",
                Org = orgId,
                ProcessIsComplete = processIsComplete,
                ExcludeConfirmedBy = orgId,
            }
        );

        IList<AltinnInstanceSummary> summaries = [];

        foreach (var instance in instances)
        {
            summaries.Add(await GetInstanceSummaryAsync(instance, appSpec));
        }

        return summaries;
    }

    public Task<IEnumerable<AltinnInstanceSummary>> GetNonCompletedInstances(
        string appId,
        bool processIsComplete = true
    )
    {
        return GetNonCompletedInstances(new AltinnAppSpecification(appId), processIsComplete);
    }

    public async Task<IEnumerable<AltinnMetadata>> GetMetadataForNonCompletedInstances(
        string appId,
        bool processIsComplete = true
    )
    {
        var orgId = altinnApiConfigurationOptions.Value.OrgId;

        var instances = await altinnStorageClient.GetAllInstances(
            new InstanceQueryParameters
            {
                AppId = $"{orgId}/{appId}",
                Org = orgId,
                ProcessIsComplete = processIsComplete,
                ExcludeConfirmedBy = orgId,
            }
        );
        return [.. instances.Select(s => s.ToAltinnMetadata())];
    }

    private async Task<AltinnInstanceSummary> GetInstanceSummaryAsync(
        AltinnInstance instance,
        AltinnAppSpecification appSpec
    )
    {
        var (mainData, structuredData, attachmentData) = appSpec.GetDataElementsBySignificance(
            instance
        );

        var attachments = new List<AltinnDocument>();

        foreach (var dataElement in attachmentData)
        {
            attachments.Add(await GetAltinnDocument(dataElement, instance, appSpec));
        }

        return new AltinnInstanceSummary
        {
            Metadata = instance.ToAltinnMetadata(),
            SkjemaAsPdf = await GetAltinnDocument(mainData, instance, appSpec),
            StructuredData = structuredData is not null
                ? await GetAltinnDocument(structuredData, instance, appSpec)
                : null,
            Attachments = attachments,
        };
    }

    private async Task<AltinnDocument> GetAltinnDocument(
        DataElement dataElement,
        AltinnInstance instance,
        AltinnAppSpecification appSpec
    )
    {
        var document = await altinnStorageClient.GetInstanceData(
            instance.CreateInstanceDataRequest(dataElement)
        );

        return new AltinnDocument
        {
            DocumentContent = document,
            FileMetadata = appSpec.CreateFileMetadata(dataElement),
        };
    }

    public async Task<AltinnSubscription?> GetAltinnSubscription(int subscriptionId)
    {
        try
        {
            return await altinnEventsClient.GetAltinnSubscription(subscriptionId);
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            throw;
        }
    }
}

file static class Extensions
{
    public static InstanceRequest CreateInstanceRequest(this AltinnInstance instance)
    {
        return new InstanceRequest
        {
            InstanceGuid = instance.GetInstanceGuid(),
            InstanceOwnerPartyId =
                instance.InstanceOwner?.PartyId
                ?? throw new InvalidOperationException("PartyId required"),
        };
    }

    public static InstanceDataRequest CreateInstanceDataRequest(
        this AltinnInstance instance,
        DataElement dataElement
    )
    {
        return new InstanceDataRequest
        {
            InstanceRequest = instance.CreateInstanceRequest(),
            DataId = Guid.Parse(
                dataElement.Id ?? throw new InvalidOperationException("Id required")
            ),
        };
    }
}
