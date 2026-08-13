using System.Net;
using Altinn.ApiClients.Dialogporten.ServiceOwner;
using Altinn.ApiClients.Dialogporten.ServiceOwner.Features.V1;
using Altinn.App.Core.Features;
using Altinn.App.Core.Internal.Instances;
using Altinn.App.Core.Internal.Process.ProcessTasks.ServiceTasks;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Arbeidstilsynet.Common.AltinnApp.Ports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

internal sealed class PatchDialogTask<T> : IServiceTask
    where T : class
{
    private readonly IServiceOwnerApi _dialogporten;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IInstanceClient _instanceClient;
    private readonly IOrganisasjonsnummerProvider<T> _organisasjonsnummerProvider;
    private readonly IPatchOperationsProvider _patchOperationsProvider;
    private readonly ILogger<PatchDialogTask<T>> _logger;

    public PatchDialogTask(
        IServiceOwnerApi dialogporten,
        IWebHostEnvironment webHostEnvironment,
        IInstanceClient instanceClient,
        IOrganisasjonsnummerProvider<T> organisasjonsnummerProvider,
        IPatchOperationsProvider patchOperationsProvider,
        ILogger<PatchDialogTask<T>> logger
    )
    {
        _dialogporten = dialogporten;
        _webHostEnvironment = webHostEnvironment;
        _instanceClient = instanceClient;
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

        var data = instance.Data.First(s => s.DataType == "skjema");
        var skjemaModel = await IInstanceDataAccessorExtensions.GetFormData<T>(
            context.InstanceDataMutator,
            new Altinn.App.Core.Models.DataElementIdentifier(data.Id)
        );
        var senderActorId =
            $"urn:altinn:organization:identifier-no:{_organisasjonsnummerProvider.GetOrganisasjonsnummer(skjemaModel)}";

        var baseUrl = _webHostEnvironment.GetPlatformBaseUrl();
        var receiptUrl =
            $"{baseUrl}/receipt/{instanceOwner}/{instanceGuid}?dontChooseReportee=true";
        var transmissionId = CreateVersion7Guid();

        //var patchOperations = _webHostEnvironment.BuildPatchOperations(dialogId, transmissionId, senderActorId, receiptUrl, instanceGuid, baseUrl);
        var patchSucceeded = await _dialogporten.PatchDialogWithFreshRevision(
            dialogId,
            _patchOperationsProvider.GetPatchOperations(),
            _logger,
            context.CancellationToken
        );
        return patchSucceeded
            ? ServiceTaskResult.Success()
            : ServiceTaskResult.FailedAbortProcessNext();
    }

    // Guid.CreateVersion7() is not available in net8.0
    private static Guid CreateVersion7Guid()
    {
        var ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Span<byte> rand = stackalloc byte[10];
        Random.Shared.NextBytes(rand);

        var tsHex = ms.ToString("x12");
        var randA = ((rand[0] & 0x0F) << 8) | rand[1];
        var varByte = (byte)(0x80 | (rand[2] & 0x3F));

        var uuid =
            $"{tsHex[..8]}-{tsHex[8..]}-7{randA:x3}-{varByte:x2}{rand[3]:x2}-{rand[4]:x2}{rand[5]:x2}{rand[6]:x2}{rand[7]:x2}{rand[8]:x2}{rand[9]:x2}";
        return Guid.Parse(uuid);
    }
}

file static class PatchDialogTaskExtensions
{
    private const int MaxPatchAttempts = 3;

    public static bool TryGetDialogId(
        this ServiceTaskContext context,
        ILogger logger,
        out Guid dialogId
    )
    {
        dialogId = Guid.Empty;

        if (
            context.InstanceDataMutator.Instance.DataValues is null
            || !context.InstanceDataMutator.Instance.DataValues.TryGetValue(
                "dialog.id",
                out var dialogIdStr
            )
            || !Guid.TryParse(dialogIdStr, out dialogId)
        )
        {
            logger.LogError("Could not retrieve dialog.id from instance DataValues.");
            return false;
        }

        return true;
    }

    public static string GetPlatformBaseUrl(this IWebHostEnvironment webHostEnvironment) =>
        webHostEnvironment.IsProduction()
            ? "https://platform.altinn.no"
            : "https://platform.tt02.altinn.no";

    public static List<JsonPatchOperation> BuildPatchOperations(
        this IWebHostEnvironment webHostEnvironment,
        Guid dialogId,
        Guid transmissionId,
        string senderActorId,
        string receiptUrl,
        Guid instanceGuid,
        string baseUrl
    )
    {
        var guiActionUrl =
            $"{webHostEnvironment.GetEndringAppBaseUrl()}/set-query-params?meldingId={instanceGuid}";

        return
        [
            new()
            {
                Op = "@\"Add\"",
                Path = "/transmissions/-",
                Value = new CreateTransmissionRequest
                {
                    Id = transmissionId,
                    Type = DialogTransmissionType.Submission,
                    Sender = new Actor
                    {
                        ActorType = ActorType.PartyRepresentative,
                        ActorId = senderActorId,
                    },
                    Content = new CreateTransmissionContent
                    {
                        Title = new ContentValue
                        {
                            MediaType = "text/plain",
                            Value =
                            [
                                new Localization
                                {
                                    LanguageCode = "nb",
                                    Value = "Innsending av forhandsmelding",
                                },
                            ],
                        },
                        ContentReference = new ContentValue
                        {
                            MediaType =
                                "application/vnd.dialogporten.frontchannelembed-url;type=text/markdown",
                            Value =
                            [
                                new Localization
                                {
                                    LanguageCode = "en",
                                    Value =
                                        $"{baseUrl}/storage/dialogporten/api/v1/receipt/{dialogId}/{transmissionId}?lang=en",
                                },
                                new Localization
                                {
                                    LanguageCode = "nb",
                                    Value =
                                        $"{baseUrl}/storage/dialogporten/api/v1/receipt/{dialogId}/{transmissionId}?lang=nb",
                                },
                                new Localization
                                {
                                    LanguageCode = "nn",
                                    Value =
                                        $"{baseUrl}/storage/dialogporten/api/v1/receipt/{dialogId}/{transmissionId}?lang=nn",
                                },
                            ],
                        },
                    },
                    NavigationalActions =
                    [
                        new CreateTransmissionNavigationalAction
                        {
                            Url = new Uri(receiptUrl),
                            Title =
                            [
                                new Localization
                                {
                                    LanguageCode = "nb",
                                    Value = "Se innsendt skjema",
                                },
                            ],
                        },
                    ],
                },
            },
            new()
            {
                Op = "@\"Replace\"",
                Path = "/guiActions",
                Value = new[]
                {
                    new CreateDialogGuiAction
                    {
                        Action = "write",
                        Url = new Uri(guiActionUrl),
                        Priority = DialogGuiActionPriority.Primary,
                        Title =
                        [
                            new Localization
                            {
                                LanguageCode = "nb",
                                Value = "Endre forhåndsmelding",
                            },
                        ],
                    },
                },
            },
        ];
    }

    public static async Task<bool> PatchDialogWithFreshRevision(
        this IServiceOwnerApi dialogporten,
        Guid dialogId,
        List<JsonPatchOperation> patchOperations,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        for (var attempt = 1; attempt <= MaxPatchAttempts; attempt++)
        {
            var dialogResponse = await dialogporten.V1.GetDialog(
                dialogId,
                null!,
                cancellationToken
            );
            if (dialogResponse is not { IsSuccessful: true })
            {
                logger.LogError(
                    "Could not retrieve dialog {DialogId} from Dialogporten before patch attempt {Attempt}. {Reason}",
                    dialogId,
                    attempt,
                    dialogResponse?.ReasonPhrase
                );
                return false;
            }

            var patchResponse = await dialogporten.V1.PatchDialog(
                dialogId,
                patchOperations,
                dialogResponse.Content.Revision,
                cancellationToken
            );
            if (patchResponse is { IsSuccessful: true })
            {
                return true;
            }

            if (attempt < MaxPatchAttempts && patchResponse.IsRetryablePatchFailure())
            {
                var retryDelay = patchResponse.GetRetryAfterDelay();
                if (retryDelay > TimeSpan.Zero)
                {
                    logger.LogWarning(
                        "Dialog {DialogId} patch attempt {Attempt} failed with retryable status {Status}. Retrying after {RetryDelay}.",
                        dialogId,
                        attempt,
                        patchResponse.StatusCode,
                        retryDelay
                    );
                    await Task.Delay(retryDelay, cancellationToken);
                }
                else
                {
                    logger.LogWarning(
                        "Dialog {DialogId} patch attempt {Attempt} failed with retryable status {Status}. Retrying with a fresh revision.",
                        dialogId,
                        attempt,
                        patchResponse.StatusCode
                    );
                }

                continue;
            }

            logger.LogError(
                "Failed to patch dialog {DialogId} on attempt {Attempt}. Status: {Status}. Body: {Body}",
                dialogId,
                attempt,
                patchResponse.StatusCode,
                patchResponse.Error?.Content
            );
            return false;
        }

        return false;
    }

    private static string GetEndringAppBaseUrl(this IWebHostEnvironment webHostEnvironment) =>
        webHostEnvironment.IsProduction()
            ? "https://dat.apps.altinn.no/dat/forhandsmelding-endring"
            : "https://dat.apps.tt02.altinn.no/dat/forhandsmelding-endring";

    private static bool IsRetryablePatchFailure(this IApiResponse response) =>
        response.StatusCode
            is HttpStatusCode.PreconditionFailed
                or HttpStatusCode.UnprocessableEntity
                or HttpStatusCode.ServiceUnavailable;

    private static TimeSpan GetRetryAfterDelay(this IApiResponse response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null)
        {
            return TimeSpan.Zero;
        }

        if (retryAfter.Delta is { } delta)
        {
            return delta;
        }

        if (retryAfter.Date is { } date)
        {
            return date - DateTimeOffset.UtcNow;
        }

        return TimeSpan.Zero;
    }
}
