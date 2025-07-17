using Altinn.App.Core.Models;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

namespace Arbeidstilsynet.Common.Altinn.Ports.Clients;

public interface IAltinnStorageClient
{
    Task<Instance> GetInstance(InstanceRequest instanceAddress);

    Task<Instance> GetInstance(CloudEvent cloudEvent);
    Task<Instance> CompleteInstance(InstanceRequest instanceAddress);
    Task<Stream> GetInstanceData(InstanceDataRequest request);
    Task<Stream> GetInstanceData(Uri absoluteUri);
}
