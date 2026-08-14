using Altinn.ApiClients.Dialogporten.ServiceOwner;
using Altinn.ApiClients.Dialogporten.ServiceOwner.Features.V1;
using Altinn.App.Core.Internal.Instances;
using Altinn.App.Core.Internal.Process.ProcessTasks.ServiceTasks;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal sealed class UpdateDialogTask<T> : IServiceTask
    where T : class
{
    private readonly IServiceOwnerApi _dialogporten;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IInstanceClient _instanceClient;
    private readonly ISubmittersOrganisasjonsnummerProvider<T> _organisasjonsnummerProvider;
    private readonly IUpdateDialogProvider<T> _updateDialogProvider;
    private readonly IPatchOperationsProvider _patchOperationsProvider;
    private readonly ILogger<UpdateDialogTask<T>> _logger;

    public UpdateDialogTask(
        IServiceOwnerApi dialogporten,
        IWebHostEnvironment webHostEnvironment,
        IInstanceClient instanceClient,
        ISubmittersOrganisasjonsnummerProvider<T> organisasjonsnummerProvider,
        IUpdateDialogProvider<T> updateDialogProvider,
        IPatchOperationsProvider patchOperationsProvider,
        ILogger<UpdateDialogTask<T>> logger
    )
    {
        _dialogporten = dialogporten;
        _webHostEnvironment = webHostEnvironment;
        _instanceClient = instanceClient;
        _organisasjonsnummerProvider = organisasjonsnummerProvider;
        _updateDialogProvider = updateDialogProvider;
        _patchOperationsProvider = patchOperationsProvider;
        _logger = logger;
    }

    public string Type => IPatchDialogConstants.UpdateDialogTaskName;

    public async Task<ServiceTaskResult> Execute(ServiceTaskContext context)
    {
        if (_webHostEnvironment.IsDevelopment())
        {
            return ServiceTaskResult.Success();
        }

        var instance = context.InstanceDataMutator.Instance;
        var instanceGuid = instance.GetInstanceGuid();
        var instanceOwner = instance.GetInstanceOwnerPartyId();

        var skjemaModel = await context.GetSkjemaFormData<T>();

        var generatedDialogId = DialogTaskHelpers.CreateVersion7Guid();
        var resolution = _updateDialogProvider.Resolve(skjemaModel, generatedDialogId);

        var resolvedDialogId = await ResolveDialogId(
            resolution,
            generatedDialogId,
            instanceOwner,
            context.CancellationToken
        );

        if (resolvedDialogId is not { } confirmedDialogId)
        {
            return ServiceTaskResult.FailedAbortProcessNext();
        }

        var senderActorId =
            $"urn:altinn:organization:identifier-no:{_organisasjonsnummerProvider.GetOrganisasjonsnummerFromSubmitter(skjemaModel)}";
        var baseUrl = _webHostEnvironment.GetPlatformBaseUrl();
        var receiptUrl =
            $"{baseUrl}/receipt/{instanceOwner}/{instanceGuid}?dontChooseReportee=true";
        var transmissionId = DialogTaskHelpers.CreateVersion7Guid();

        var patchSucceeded = await _dialogporten.PatchDialogWithFreshRevision(
            confirmedDialogId,
            _patchOperationsProvider.GetPatchOperations(
                _webHostEnvironment,
                confirmedDialogId,
                transmissionId,
                senderActorId,
                receiptUrl,
                instanceGuid,
                baseUrl
            ),
            _logger,
            context.CancellationToken
        );

        if (!patchSucceeded)
        {
            return ServiceTaskResult.FailedAbortProcessNext();
        }

        var persisted = await _instanceClient.PersistDialogId(
            instance,
            confirmedDialogId,
            _logger
        );

        return persisted
            ? ServiceTaskResult.Success()
            : ServiceTaskResult.FailedAbortProcessNext();
    }

    private async Task<Guid?> ResolveDialogId(
        DialogResolution resolution,
        Guid generatedDialogId,
        int instanceOwner,
        CancellationToken cancellationToken
    )
    {
        switch (resolution)
        {
            case DialogResolution.CreateNew createNew:
            {
                var createResponse = await _dialogporten.V1.CreateDialog(
                    createNew.Request,
                    cancellationToken
                );
                if (createResponse is not { IsSuccessful: true })
                {
                    _logger.LogError(
                        "Failed to create dialog. Status: {Status}. Body: {Body}",
                        createResponse?.StatusCode,
                        createResponse?.Error?.Content
                    );
                    return null;
                }

                return createNew.Request.Id ?? generatedDialogId;
            }
            case DialogResolution.ReuseExisting reuseExisting:
            {
                var lookupResponse = await _dialogporten.V1.GetDialogLookup(
                    $"urn:altinn:instance-id:{instanceOwner}/{reuseExisting.MeldingId}",
                    new AcceptedLanguages
                    {
                        AcceptedLanguage = [new AcceptedLanguage { LanguageCode = "nb" }],
                    },
                    cancellationToken
                );
                if (lookupResponse is not { IsSuccessful: true })
                {
                    _logger.LogError(
                        "Could not retrieve dialog id from Dialogporten. {Reason}",
                        lookupResponse?.ReasonPhrase
                    );
                    return null;
                }

                return lookupResponse.Content.DialogId;
            }
            default:
                _logger.LogError(
                    "Unknown dialog resolution type {Type}.",
                    resolution.GetType().Name
                );
                return null;
        }
    }
}
