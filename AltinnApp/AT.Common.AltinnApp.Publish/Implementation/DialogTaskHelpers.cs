using System.Net;
using Altinn.ApiClients.Dialogporten.ServiceOwner;
using Altinn.ApiClients.Dialogporten.ServiceOwner.Features.V1;
using Altinn.App.Core.Features;
using Altinn.App.Core.Internal.Instances;
using Altinn.App.Core.Internal.Process.ProcessTasks.ServiceTasks;
using Altinn.App.Core.Models;
using Altinn.Platform.Storage.Interface.Models;
using Arbeidstilsynet.Common.AltinnApp.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;

namespace Arbeidstilsynet.Common.AltinnApp.Implementation;

/// <summary>
/// Shared helpers used by the Dialogporten related service tasks
/// (<see cref="PatchDialogTask{T}"/> and <see cref="UpdateDialogTask{T}"/>).
/// </summary>
internal static class DialogTaskHelpers
{
    private const int MaxPatchAttempts = 3;
    internal const string DialogIdDataValueKey = "dialog.id";
    internal const string SkjemaDataType = "skjema";

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
                DialogIdDataValueKey,
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

    public static async Task<T> GetSkjemaFormData<T>(this ServiceTaskContext context)
        where T : class
    {
        var data = context.InstanceDataMutator.Instance.Data.First(s =>
            s.DataType == SkjemaDataType
        );
        return await IInstanceDataAccessorExtensions.GetFormData<T>(
            context.InstanceDataMutator,
            new DataElementIdentifier(data.Id)
        );
    }

    public static async Task<bool> PersistDialogId(
        this IInstanceClient instanceClient,
        Instance instance,
        Guid dialogId,
        ILogger logger
    )
    {
        var updatedInstance = await instanceClient.UpdateDataValue(
            instance,
            DialogIdDataValueKey,
            dialogId.ToString()
        );

        if (
            updatedInstance.DataValues is not null
            && updatedInstance.DataValues.TryGetValue(
                DialogIdDataValueKey,
                out var persistedDialogId
            )
            && string.Equals(
                persistedDialogId,
                dialogId.ToString(),
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return true;
        }

        logger.LogError(
            "Storage did not persist dialog.id for instance {InstanceId}. Expected {ExpectedDialogId}.",
            instance.Id,
            dialogId
        );

        return false;
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

    public static string GetPlatformBaseUrl(this IWebHostEnvironment webHostEnvironment) =>
        webHostEnvironment.IsProduction()
            ? "https://platform.altinn.no"
            : "https://platform.tt02.altinn.no";

    // Guid.CreateVersion7() is not available in net8.0
    public static Guid CreateVersion7Guid()
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
