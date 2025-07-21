using Altinn.App.Core.Infrastructure.Clients.Events;
using Altinn.App.Core.Models;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.AspNetCore.Hosting;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnAdapter(
    IAltinnStorageClient altinnStorageClient,
    IAltinnEventsClient altinnEventsClient
) : IAltinnAdapter
{
    public async Task<AltinnInstanceSummary> GetSummary(
        CloudEvent cloudEvent,
        Action<AltinnAppConfiguration>? appConfigAction = null
    )
    {
        var appConfig = new AltinnAppConfiguration();
        appConfigAction?.Invoke(appConfig);
        // get instans basert på cloud event data
        var instance = await altinnStorageClient.GetInstance(cloudEvent);

        return await GetInstanceSummaryAsync(instance, appConfig.MainDocumentDataTypeName);
    }

    public async Task<Subscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto,
        IWebHostEnvironment webHostEnvironment
    )
    {
        return await altinnEventsClient.Subscribe(
            new SubscriptionRequest()
            {
                SourceFilter = new Uri(
                    new Uri(
                        webHostEnvironment.GetAltinnAppBaseUrl(
                            DependencyInjectionExtensions.AltinnOrgIdentifier
                        )
                    ),
                    subscriptionRequestDto.AltinnAppIdentifier
                ),
                EndPoint = subscriptionRequestDto.CallbackUrl,
                TypeFilter = "app.instance.process.completed",
            }
        );
    }

    private async Task<Stream> GetInstanceData(DataElement dataElement, Instance instance)
    {
        return await altinnStorageClient.GetInstanceData(
            new InstanceDataRequest
            {
                InstanceRequest = CreateInstanceRequest(dataElement, instance),
                DataId = Guid.Parse(
                    dataElement.Id ?? throw new InvalidOperationException("Id required")
                ),
            }
        );
    }

    private static InstanceRequest CreateInstanceRequest(DataElement dataElement, Instance instance)
    {
        return new InstanceRequest
        {
            InstanceGuid = Guid.Parse(
                dataElement.InstanceGuid
                    ?? throw new InvalidOperationException("InstanceGuid required")
            ),
            InstanceOwnerPartyId =
                instance.InstanceOwner?.PartyId
                ?? throw new InvalidOperationException("PartyId required"),
        };
    }

    private static FileMetadata MapDatumToAltinnMetadataSummary(
        DataElement dataElement,
        string mainDocumentDataTypeName
    )
    {
        return new FileMetadata()
        {
            ContentType = dataElement.ContentType,
            DataType = dataElement.DataType,
            Filename = string.Equals(mainDocumentDataTypeName, dataElement.DataType)
                ? "MainDocument"
                : dataElement.Filename,
            FileScanResult = dataElement.FileScanResult.ToString(),
        };
    }

    public async Task<AltinnInstanceSummary[]> GetNonCompletedInstances(
        string appId,
        bool? processIsComplete = true,
        string? excludeConfirmedBy = DependencyInjectionExtensions.AltinnOrgIdentifier,
        Action<AltinnAppConfiguration>? appConfigAction = null
    )
    {
        var appConfig = new AltinnAppConfiguration();
        appConfigAction?.Invoke(appConfig);
        var instances = await altinnStorageClient.GetInstances(
            new InstanceQueryParameters
            {
                AppId = $"{DependencyInjectionExtensions.AltinnOrgIdentifier}/{appId}",
                Org = DependencyInjectionExtensions.AltinnOrgIdentifier,
                ProcessIsComplete = processIsComplete,
                ExcludeConfirmedBy =
                    excludeConfirmedBy ?? DependencyInjectionExtensions.AltinnOrgIdentifier,
            }
        );
        return await Task.WhenAll(
            instances
                ?.Instances?.Select(async s =>
                    await GetInstanceSummaryAsync(s, appConfig.MainDocumentDataTypeName)
                )
                .ToList() ?? await Task.FromResult(new List<Task<AltinnInstanceSummary>>())
        );
    }

    private async Task<AltinnInstanceSummary> GetInstanceSummaryAsync(
        Instance instance,
        string mainDocumentDataTypeName
    )
    {
        var documents = await Task.WhenAll(
            instance.Data.Select(async d => new AltinnDocument
            {
                DocumentContent = await GetInstanceData(d, instance),
                IsMainDocument = d.DataType == mainDocumentDataTypeName,
                FileMetadata = MapDatumToAltinnMetadataSummary(d, mainDocumentDataTypeName),
            })
        );

        return new AltinnInstanceSummary
        {
            Metadata = instance.ToAltinnMetadata(),
            AltinnSkjema = documents.First(d => d.IsMainDocument),
            Attachments = [.. documents.Where(d => !d.IsMainDocument)],
        };
    }
}
