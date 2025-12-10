using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Extensions;
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
    IOptions<AltinnApiConfiguration> altinnApiConfigurationOptions,
    ILogger<AltinnAdapter> logger
) : IAltinnAdapter
{
    public async Task<AltinnInstanceSummary> GetSummary(
        AltinnCloudEvent cloudEvent,
        AltinnAppConfiguration? appConfig = null
    )
    {
        appConfig ??= new AltinnAppConfiguration();
        // get instans basert på cloud event data
        var instance = await altinnStorageClient.GetInstance(cloudEvent);

        return await GetInstanceSummaryAsync(instance, appConfig.MainDocumentDataTypeName);
    }

    public Task<AltinnSubscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto
    )
    {
        var mappedRequest = new AltinnSubscriptionRequest()
        {
            SourceFilter = new Uri(
                altinnApiConfigurationOptions.Value.AppBaseUrl,
                $"{DependencyInjectionExtensions.AltinnOrgIdentifier}/{subscriptionRequestDto.AltinnAppIdentifier}"
            ),
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

    public async Task<IEnumerable<AltinnInstanceSummary>> GetNonCompletedInstances(
        string appId,
        bool processIsComplete = true,
        string? excludeConfirmedBy = DependencyInjectionExtensions.AltinnOrgIdentifier,
        AltinnAppConfiguration? appConfig = null
    )
    {
        appConfig ??= new AltinnAppConfiguration();

        var instances = await altinnStorageClient.GetAllInstances(
            new InstanceQueryParameters
            {
                AppId = $"{DependencyInjectionExtensions.AltinnOrgIdentifier}/{appId}",
                Org = DependencyInjectionExtensions.AltinnOrgIdentifier,
                ProcessIsComplete = processIsComplete,
                ExcludeConfirmedBy =
                    excludeConfirmedBy ?? DependencyInjectionExtensions.AltinnOrgIdentifier,
            }
        );

        IList<AltinnInstanceSummary> summaries = [];
        foreach (var instance in instances)
        {
            summaries.Add(
                await GetInstanceSummaryAsync(instance, appConfig.MainDocumentDataTypeName)
            );
        }

        return summaries;
    }

    public async Task<IEnumerable<AltinnMetadata>> GetMetadataForNonCompletedInstances(
        string appId,
        bool processIsComplete = true,
        string? excludeConfirmedBy = DependencyInjectionExtensions.AltinnOrgIdentifier
    )
    {
        var instances = await altinnStorageClient.GetAllInstances(
            new InstanceQueryParameters
            {
                AppId = $"{DependencyInjectionExtensions.AltinnOrgIdentifier}/{appId}",
                Org = DependencyInjectionExtensions.AltinnOrgIdentifier,
                ProcessIsComplete = processIsComplete,
                ExcludeConfirmedBy =
                    excludeConfirmedBy ?? DependencyInjectionExtensions.AltinnOrgIdentifier,
            }
        );

        return [.. instances.Select(s => s.ToAltinnMetadata())];
    }

    private async Task<AltinnInstanceSummary> GetInstanceSummaryAsync(
        AltinnInstance instance,
        string mainDocumentDataTypeName
    )
    {
        IList<AltinnDocument> documents = [];
        foreach (var dataElement in instance.Data)
        {
            documents.Add(await GetAltinnDocument(dataElement, instance, mainDocumentDataTypeName));
        }

        return new AltinnInstanceSummary
        {
            Metadata = instance.ToAltinnMetadata(),
            AltinnSkjema = documents.First(d => d.IsMainDocument),
            Attachments = [.. documents.Where(d => !d.IsMainDocument)],
        };
    }

    private async Task<AltinnDocument> GetAltinnDocument(
        DataElement dataElement,
        AltinnInstance instance,
        string mainDocumentDataTypeName
    )
    {
        var document = await altinnStorageClient.GetInstanceData(
            instance.CreateInstanceDataRequest(dataElement)
        );

        return new AltinnDocument
        {
            DocumentContent = document,
            IsMainDocument = dataElement.DataType == mainDocumentDataTypeName,
            FileMetadata = dataElement.ToFileMetadata(mainDocumentDataTypeName),
        };
    }
}

file static class Extensions
{
    public static FileMetadata ToFileMetadata(
        this DataElement dataElement,
        string mainDocumentDataTypeName
    )
    {
        return new FileMetadata
        {
            ContentType = dataElement.ContentType,
            DataType = dataElement.DataType,
            Filename = string.Equals(mainDocumentDataTypeName, dataElement.DataType)
                ? "MainDocument"
                : dataElement.Filename,
            FileScanResult = dataElement.FileScanResult,
        };
    }

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
