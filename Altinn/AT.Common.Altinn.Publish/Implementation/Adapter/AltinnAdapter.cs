using System.Net;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Events.Models;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Exceptions;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Storage.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnAdapter(
    IAltinnStorageClient altinnStorageClient,
    IAltinnEventsClient altinnEventsClient,
    IOptions<AltinnConfiguration> altinnConfigurationOptions,
    ResolvedAltinnUrls altinnUrls,
    ILogger<AltinnAdapter> logger
) : IAltinnAdapter
{
    public async Task<AltinnInstanceSummary> GetSummary(AltinnCloudEvent cloudEvent)
    {
        var instance = await altinnStorageClient.GetInstance(cloudEvent);

        return await GetInstanceSummaryAsync(instance);
    }

    public Task<Subscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto
    )
    {
        var baseUrl = altinnUrls.AppBaseUrl;
        var orgId = altinnConfigurationOptions.Value.OrgId;
        var appId = subscriptionRequestDto.AltinnAppId;

        var mappedRequest = new SubscriptionRequestModel()
        {
            SourceFilter = new Uri(baseUrl, $"{orgId}/{appId}").ToString(),
            EndPoint = subscriptionRequestDto.CallbackUrl.ToString(),
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

    public async Task<bool> UnsubscribeForCompletedProcessEvents(Subscription altinnSubscription)
    {
        if (altinnSubscription.Id is not { } subscriptionId)
        {
            return false;
        }

        try
        {
            await altinnEventsClient.Unsubscribe(subscriptionId);
            return true;
        }
        catch (ApiException e) when (e.ResponseStatusCode == (int)HttpStatusCode.NotFound)
        {
            return false;
        }
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
            try
            {
                summaries.Add(await GetInstanceSummaryAsync(instance));
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogWarning(
                    e,
                    "Failed to get summary for instance {InstanceId} from app {AppId}.",
                    instance.Id,
                    instance.AppId
                );
            }
        }

        return summaries;
    }

    private async Task<AltinnInstanceSummary> GetInstanceSummaryAsync(Instance instance)
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

    private async Task<AltinnDocument> GetAltinnDocument(DataElement dataElement, Instance instance)
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

    public async Task<Subscription?> GetAltinnSubscription(int subscriptionId)
    {
        try
        {
            return await altinnEventsClient.GetAltinnSubscription(subscriptionId);
        }
        catch (ApiException e) when (e.ResponseStatusCode == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}

file static class Extensions
{
    public static InstanceRequest CreateInstanceRequest(this Instance instance)
    {
        return new InstanceRequest
        {
            InstanceGuid = instance.GetInstanceGuid(),
            InstanceOwnerPartyId =
                instance.InstanceOwner?.PartyId
                ?? throw new AltinnInstanceOwnerPartyIdMissingException(instance),
        };
    }

    public static InstanceDataRequest CreateInstanceDataRequest(
        this Instance instance,
        DataElement dataElement
    )
    {
        return new InstanceDataRequest
        {
            InstanceRequest = instance.CreateInstanceRequest(),
            DataId = Guid.Parse(
                dataElement.Id
                    ?? throw new AltinnDataElementIdMissingException(instance, dataElement)
            ),
        };
    }
}
