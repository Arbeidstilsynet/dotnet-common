using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

internal static class CorrespondenceRequestExtensions
{
    /// <summary>
    /// Maps the flat <see cref="CorrespondenceRequest"/> to the nested JSON structure
    /// expected by the Altinn Correspondence API (InitializeCorrespondencesExt).
    /// </summary>
    public static InitializeCorrespondencesExt ToApiRequest(this CorrespondenceRequest request)
    {
        return new InitializeCorrespondencesExt
        {
            Correspondence = new BaseCorrespondenceExt
            {
                ResourceId = $"urn:altinn:resource:{request.ResourceIdentifier}",
                SendersReference = request.SendersReference,
                MessageSender = request.MessageSender,
                Content = request.Content,
                RequestedPublishTime = request.RequestedPublishTime,
                DueDateTime = request.DueDateTime,
                ExternalReferences = request.ExternalReferences,
                PropertyList = new BaseCorrespondenceExt_propertyList
                {
                    AdditionalData = request.PropertyList.ToDictionary(
                        entry => entry.Key,
                        entry => (object)entry.Value
                    ),
                },
                ReplyOptions = request.ReplyOptions,
                Notification = request.Notification,
                IgnoreReservation = request.IgnoreReservation,
                IsConfirmationNeeded = request.IsConfirmationNeeded,
                IsConfidential = request.IsConfidential,
            },
            Recipients = request.Recipients.ToReceiverList(),
            ExistingAttachments = request.ExistingAttachments ?? [],
            IdempotentKey = request.IdempotentKey,
        };
    }

    public static List<string> ToReceiverList(this List<IAltinnRecipient> receivers)
    {
        return [.. receivers.Select(s => s.ToAltinnRessourceFormat())];
    }
}

