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
    IOptions<AltinnConfiguration> altinnConfigurationOptions,
    ILogger<AltinnAdapter> logger
) : IAltinnAdapter
{
    public async Task<AltinnInstanceSummary> GetSummary(AltinnCloudEvent cloudEvent)
    {
        var instance = await altinnStorageClient.GetInstance(cloudEvent);

        return await GetInstanceSummaryAsync(instance);
    }

    public Task<AltinnSubscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto
    )
    {
        var baseUrl = altinnConfigurationOptions.Value.AppBaseUrl;
        var orgId = altinnConfigurationOptions.Value.OrgId;
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

    public async Task<IEnumerable<AltinnMetadata>> GetMetadataForNonCompletedInstances(
        string appId,
        bool processIsComplete = true
    )
    {
        var orgId = altinnConfigurationOptions.Value.OrgId;

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

    public async Task<IEnumerable<AltinnInstanceSummary>> GetNonCompletedInstances(
        string appId,
        bool processIsComplete = true
    )
    {
        var orgId = altinnConfigurationOptions.Value.OrgId;

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
            summaries.Add(await GetInstanceSummaryAsync(instance));
        }

        return summaries;
    }

    private async Task<AltinnInstanceSummary> GetInstanceSummaryAsync(AltinnInstance instance)
    {
        var (mainData, structuredData, attachmentData) = instance.GetDataElementsBySignificance();

        var attachments = new List<AltinnDocument>();

        foreach (var dataElement in attachmentData)
        {
            attachments.Add(await GetAltinnDocument(dataElement, instance));
        }

        return new AltinnInstanceSummary
        {
            Metadata = instance.ToAltinnMetadata(),
            SkjemaAsPdf = await GetAltinnDocument(mainData, instance),
            StructuredData = structuredData is not null
                ? await GetAltinnDocument(structuredData, instance)
                : null,
            Attachments = attachments,
        };
    }

    private async Task<AltinnDocument> GetAltinnDocument(
        DataElement dataElement,
        AltinnInstance instance
    )
    {
        var appSpec = instance.GetSpecification();

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
