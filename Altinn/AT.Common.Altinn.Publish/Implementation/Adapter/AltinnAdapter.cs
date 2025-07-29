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
using Microsoft.Extensions.Options;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Adapter;

internal class AltinnAdapter(
    IAltinnStorageClient altinnStorageClient,
    IAltinnEventsClient altinnEventsClient,
    IOptions<AltinnApiConfiguration> altinnApiConfigurationOptions
) : IAltinnAdapter
{
    public async Task<AltinnInstanceSummary> GetSummary(
        CloudEvent cloudEvent,
        AltinnAppConfiguration? appConfig = null
    )
    {
        appConfig ??= new AltinnAppConfiguration();
        // get instans basert på cloud event data
        var instance = await altinnStorageClient.GetInstance(cloudEvent);

        return await GetInstanceSummaryAsync(instance, appConfig.MainDocumentDataTypeName);
    }

    public Task<Subscription> SubscribeForCompletedProcessEvents(
        SubscriptionRequestDto subscriptionRequestDto
    )
    {
        return altinnEventsClient.Subscribe(
            new SubscriptionRequest()
            {
                SourceFilter = new Uri(
                    altinnApiConfigurationOptions.Value.AppBaseUrl,
                    subscriptionRequestDto.AltinnAppIdentifier
                ),
                EndPoint = subscriptionRequestDto.CallbackUrl,
                TypeFilter = "app.instance.process.completed",
            }
        );
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
        
        var fetchInstanceTasks  = instances
            .Select(instance => GetInstanceSummaryAsync(instance, appConfig.MainDocumentDataTypeName));
        
        return await Task.WhenAll(fetchInstanceTasks);
    }

    private async Task<AltinnInstanceSummary> GetInstanceSummaryAsync(
        Instance instance,
        string mainDocumentDataTypeName
    )
    {
        var documents = await Task.WhenAll(
            instance.Data.Select(dataElement => GetAltinnDocument(dataElement, instance, mainDocumentDataTypeName))
        );

        return new AltinnInstanceSummary
        {
            Metadata = instance.ToAltinnMetadata(),
            AltinnSkjema = documents.First(d => d.IsMainDocument),
            Attachments = [.. documents.Where(d => !d.IsMainDocument)],
        };
    }
    
    private async Task<AltinnDocument> GetAltinnDocument(DataElement dataElement, Instance instance, string mainDocumentDataTypeName)
    {
        var document = await altinnStorageClient.GetInstanceData(instance.CreateInstanceDataRequest(dataElement));
        
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
    public static FileMetadata ToFileMetadata(this DataElement dataElement, string mainDocumentDataTypeName)
    {
        return new FileMetadata
        {
            ContentType = dataElement.ContentType,
            DataType = dataElement.DataType,
            Filename = string.Equals(mainDocumentDataTypeName, dataElement.DataType)
                ? "MainDocument"
                : dataElement.Filename,
            FileScanResult = dataElement.FileScanResult.ToString(),
        };
    }
    
    public static InstanceRequest CreateInstanceRequest(this Instance instance)
    {
        return new InstanceRequest
        {
            InstanceGuid = instance.GetInstanceGuid(),
            InstanceOwnerPartyId = instance.InstanceOwner?.PartyId
                                   ?? throw new InvalidOperationException("PartyId required"),
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
            DataId = Guid.Parse(dataElement.Id ?? throw new InvalidOperationException("Id required")),
        };
    }
    
    
}