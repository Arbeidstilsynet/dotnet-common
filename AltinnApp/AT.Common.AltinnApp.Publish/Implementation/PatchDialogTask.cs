using Altinn.ApiClients.Dialogporten.ServiceOwner;
using Altinn.App.Core.Internal.Process.ProcessTasks.ServiceTasks;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal sealed class PatchDialogTask<T> : IServiceTask
    where T : class
{
    private readonly IServiceOwnerApi _dialogporten;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ISubmittersOrganisasjonsnummerProvider<T> _organisasjonsnummerProvider;
    private readonly IPatchOperationsProvider _patchOperationsProvider;
    private readonly ILogger<PatchDialogTask<T>> _logger;

    public PatchDialogTask(
        IServiceOwnerApi dialogporten,
        IWebHostEnvironment webHostEnvironment,
        ISubmittersOrganisasjonsnummerProvider<T> organisasjonsnummerProvider,
        IPatchOperationsProvider patchOperationsProvider,
        ILogger<PatchDialogTask<T>> logger
    )
    {
        _dialogporten = dialogporten;
        _webHostEnvironment = webHostEnvironment;
        _organisasjonsnummerProvider = organisasjonsnummerProvider;
        _patchOperationsProvider = patchOperationsProvider;
        _logger = logger;
    }

    public string Type => IPatchDialogConstants.PatchDialogTaskName;

    public async Task<ServiceTaskResult> Execute(ServiceTaskContext context)
    {
        if (_webHostEnvironment.IsDevelopment())
        {
            return ServiceTaskResult.Success();
        }

        if (!context.TryGetDialogId(_logger, out var dialogId))
        {
            return ServiceTaskResult.FailedAbortProcessNext();
        }

        var instance = context.InstanceDataMutator.Instance;
        var instanceGuid = instance.GetInstanceGuid();
        var instanceOwner = instance.GetInstanceOwnerPartyId();

        var skjemaModel = await context.GetSkjemaFormData<T>();
        var senderActorId =
            $"urn:altinn:organization:identifier-no:{_organisasjonsnummerProvider.GetOrganisasjonsnummerFromSubmitter(skjemaModel)}";

        var baseUrl = _webHostEnvironment.GetPlatformBaseUrl();
        var receiptUrl =
            $"{baseUrl}/receipt/{instanceOwner}/{instanceGuid}?dontChooseReportee=true";
        var transmissionId = DialogTaskHelpers.CreateVersion7Guid();

        var patchSucceeded = await _dialogporten.PatchDialogWithFreshRevision(
            dialogId,
            _patchOperationsProvider.GetPatchOperations(
                _webHostEnvironment,
                dialogId,
                transmissionId,
                senderActorId,
                receiptUrl,
                instanceGuid,
                baseUrl
            ),
            _logger,
            context.CancellationToken
        );
        return patchSucceeded
            ? ServiceTaskResult.Success()
            : ServiceTaskResult.FailedAbortProcessNext();
    }
}
