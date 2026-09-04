using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Generated = Arbeidstilsynet.Common.Altinn.Correspondence.Models;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Mapping;

/// <summary>
/// Maps between the package's correspondence models and the generated ones.
/// </summary>
internal static class CorrespondenceMappings
{
    public static Generated.InitializeCorrespondencesExt ToGenerated(
        this InitializeCorrespondences source
    )
    {
        return new Generated.InitializeCorrespondencesExt
        {
            Correspondence = source.Correspondence.ToGenerated(),
            Recipients = source.Recipients,
            ExistingAttachments = [.. source.ExistingAttachments.Select(id => (Guid?)id)],
            IdempotentKey = source.IdempotentKey,
        };
    }

    private static Generated.BaseCorrespondenceExt ToGenerated(this BaseCorrespondence source)
    {
        return new Generated.BaseCorrespondenceExt
        {
            ResourceId = source.ResourceId,
            Sender = source.Sender,
            SendersReference = source.SendersReference,
            MessageSender = source.MessageSender,
            RequestedPublishTime = source.RequestedPublishTime,
            DueDateTime = source.DueDateTime,
            IgnoreReservation = source.IgnoreReservation,
            IsConfirmationNeeded = source.IsConfirmationNeeded,
            IsConfidential = source.IsConfidential,
            Content = source.Content is { } content
                ? new Generated.InitializeCorrespondenceContentExt
                {
                    Language = content.Language,
                    MessageTitle = content.MessageTitle,
                    MessageSummary = content.MessageSummary,
                    MessageBody = content.MessageBody,
                    Attachments =
                    [
                        .. content.Attachments.Select(
                            attachment => new Generated.InitializeCorrespondenceAttachmentExt
                            {
                                FileName = attachment.FileName,
                                DisplayName = attachment.DisplayName,
                                IsEncrypted = attachment.IsEncrypted,
                                Checksum = attachment.Checksum,
                                SendersReference = attachment.SendersReference,
                                ExpirationInDays = attachment.ExpirationInDays,
                            }
                        ),
                    ],
                }
                : null,
            ExternalReferences =
            [
                .. (source.ExternalReferences ?? []).Select(
                    reference => new Generated.ExternalReferenceExt
                    {
                        ReferenceValue = reference.ReferenceValue,
                        ReferenceType = Enum.TryParse<Generated.ReferenceTypeExt>(
                            reference.ReferenceType.ToString(),
                            out var referenceType
                        )
                            ? referenceType
                            : null,
                    }
                ),
            ],
            ReplyOptions =
            [
                .. (source.ReplyOptions ?? []).Select(
                    option => new Generated.CorrespondenceReplyOptionExt
                    {
                        LinkURL = option.LinkURL,
                        LinkText = option.LinkText,
                    }
                ),
            ],
            PropertyList = new Generated.BaseCorrespondenceExt_propertyList
            {
                AdditionalData = source.PropertyList.ToDictionary(
                    entry => entry.Key,
                    entry => (object)entry.Value
                ),
            },
        };
    }

    public static AltinnCorrespondenceOverview ToOverview(
        this Generated.CorrespondenceOverviewExt source
    )
    {
        return new AltinnCorrespondenceOverview
        {
            CorrespondenceId = source.CorrespondenceId ?? Guid.Empty,
            Recipient = source.Recipient ?? string.Empty,
            ResourceId = source.ResourceId ?? string.Empty,
            Sender = source.Sender,
            SendersReference = source.SendersReference ?? string.Empty,
            MessageSender = source.MessageSender,
            Created = source.Created ?? default,
            Published = source.Published,
            RequestedPublishTime = source.RequestedPublishTime,
            DueDateTime = source.DueDateTime,
            Status = ParseEnum(source.Status?.ToString(), CorrespondenceStatus.Initialized),
            StatusText = source.StatusText ?? string.Empty,
            StatusChanged = source.StatusChanged ?? default,
            Altinn2CorrespondenceId = source.Altinn2CorrespondenceId,
            IgnoreReservation = source.IgnoreReservation,
            IsConfirmationNeeded = source.IsConfirmationNeeded ?? false,
            IsConfidential = source.IsConfidential ?? false,
            PropertyList = ToStringDictionary(source.PropertyList?.AdditionalData),
        };
    }

    public static CorrespondenceResponse ToResponse(
        this Generated.InitializeCorrespondencesResponseExt source
    )
    {
        return new CorrespondenceResponse
        {
            AttachmentIds =
            [
                .. (source.AttachmentIds ?? []).Where(id => id.HasValue).Select(id => id!.Value),
            ],
            Correspondences =
            [
                .. (source.Correspondences ?? []).Select(
                    correspondence => new InitializedCorrespondences
                    {
                        CorrespondenceId = correspondence.CorrespondenceId ?? Guid.Empty,
                        Recipient = correspondence.Recipient ?? string.Empty,
                        Status = ParseEnum(
                            correspondence.Status?.ToString(),
                            CorrespondenceStatus.Initialized
                        ),
                    }
                ),
            ],
        };
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value, out var parsed) ? parsed : fallback;

    private static Dictionary<string, string> ToStringDictionary(
        IDictionary<string, object>? additionalData
    )
    {
        if (additionalData is null)
        {
            return [];
        }

        return additionalData
            .Where(entry => entry.Value is not null)
            .ToDictionary(entry => entry.Key, entry => entry.Value.ToString() ?? string.Empty);
    }
}
