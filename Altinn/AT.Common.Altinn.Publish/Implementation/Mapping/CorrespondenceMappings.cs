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
            Notification = source.Notification?.ToGenerated(),
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
            Content = source.Content is { } content
                ? new CorrespondenceContent
                {
                    Language = content.Language,
                    MessageTitle = content.MessageTitle ?? string.Empty,
                    MessageSummary = content.MessageSummary,
                    MessageBody = content.MessageBody ?? string.Empty,
                    Attachments =
                    [
                        .. (content.Attachments ?? []).Select(
                            attachment => new CorrespondenceAttachment
                            {
                                Id = attachment.Id ?? Guid.Empty,
                                FileName = attachment.FileName,
                                DisplayName = attachment.DisplayName,
                                IsEncrypted = attachment.IsEncrypted ?? false,
                                Checksum = attachment.Checksum,
                                SendersReference = attachment.SendersReference ?? string.Empty,
                                ExpirationInDays = attachment.ExpirationInDays,
                                Created = attachment.Created ?? default,
                                DataLocationType = ParseEnum(
                                    attachment.DataLocationType?.ToString(),
                                    AttachmentDataLocationType.AltinnCorrespondenceAttachment
                                ),
                                Status = ParseEnum(
                                    attachment.Status?.ToString(),
                                    AttachmentStatus.Initialized
                                ),
                                StatusText = attachment.StatusText ?? string.Empty,
                                StatusChanged = attachment.StatusChanged ?? default,
                                DataType = attachment.DataType,
                                ExpirationTime = attachment.ExpirationTime,
                            }
                        ),
                    ],
                }
                : null,
            ExternalReferences =
            [
                .. (source.ExternalReferences ?? []).Select(reference => new ExternalReference
                {
                    ReferenceValue = reference.ReferenceValue ?? string.Empty,
                    ReferenceType = ParseEnum(
                        reference.ReferenceType?.ToString(),
                        ReferenceType.Generic
                    ),
                }),
            ],
            ReplyOptions =
            [
                .. (source.ReplyOptions ?? []).Select(option => new CorrespondenceReplyOption
                {
                    LinkURL = option.LinkURL ?? string.Empty,
                    LinkText = option.LinkText,
                }),
            ],
            Notification = source.Notification?.ToModel(),
            Notifications =
            [
                .. (source.Notifications ?? []).Select(
                    notification => new CorrespondenceNotificationOverview
                    {
                        NotificationOrderId = notification.NotificationOrderId,
                        IsReminder = notification.IsReminder ?? false,
                    }
                ),
            ],
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
                        Notifications =
                        [
                            .. (correspondence.Notifications ?? []).Select(
                                notification => new InitializedCorrespondencesNotifications
                                {
                                    OrderId = notification.OrderId,
                                    IsReminder = notification.IsReminder,
                                    Status = ParseEnum(
                                        notification.Status?.ToString(),
                                        InitializedNotificationStatus.Failure
                                    ),
                                }
                            ),
                        ],
                    }
                ),
            ],
        };
    }

    private static Generated.InitializeCorrespondenceNotificationExt ToGenerated(
        this InitializeCorrespondenceNotification source
    )
    {
        return new Generated.InitializeCorrespondenceNotificationExt
        {
            NotificationTemplate = ParseNullableEnum<Generated.NotificationTemplateExt>(
                source.NotificationTemplate?.ToString()
            ),
            EmailSubject = source.EmailSubject,
            EmailBody = source.EmailBody,
            EmailContentType = ParseNullableEnum<Generated.EmailContentType>(
                source.EmailContentType.ToString()
            ),
            SmsBody = source.SmsBody,
            SendReminder = source.SendReminder,
            ReminderEmailSubject = source.ReminderEmailSubject,
            ReminderEmailBody = source.ReminderEmailBody,
            ReminderEmailContentType = ParseNullableEnum<Generated.EmailContentType>(
                source.ReminderEmailContentType?.ToString()
            ),
            ReminderSmsBody = source.ReminderSmsBody,
            NotificationChannel = ParseNullableEnum<Generated.NotificationChannelExt>(
                source.NotificationChannel.ToString()
            ),
            ReminderNotificationChannel = ParseNullableEnum<Generated.NotificationChannelExt>(
                source.ReminderNotificationChannel?.ToString()
            ),
            SendersReference = source.SendersReference,
            CustomRecipients =
            [
                .. (source.CustomRecipients ?? []).Select(
                    recipient => new Generated.NotificationRecipientExt
                    {
                        EmailAddress = recipient.EmailAddress,
                        MobileNumber = recipient.MobileNumber,
                        OrganizationNumber = recipient.OrganizationNumber,
                        NationalIdentityNumber = recipient.NationalIdentityNumber,
                        IsReserved = recipient.IsReserved,
                    }
                ),
            ],
            OverrideRegisteredContactInformation = source.OverrideRegisteredContactInformation,
        };
    }

    private static InitializeCorrespondenceNotification ToModel(
        this Generated.InitializeCorrespondenceNotificationExt source
    )
    {
        return new InitializeCorrespondenceNotification
        {
            NotificationTemplate = ParseNullableEnum<NotificationTemplate>(
                source.NotificationTemplate?.ToString()
            ),
            EmailSubject = source.EmailSubject,
            EmailBody = source.EmailBody,
            EmailContentType = ParseEnum(
                source.EmailContentType?.ToString(),
                EmailContentType.Plain
            ),
            SmsBody = source.SmsBody,
            SendReminder = source.SendReminder ?? false,
            ReminderEmailSubject = source.ReminderEmailSubject,
            ReminderEmailBody = source.ReminderEmailBody,
            ReminderEmailContentType = ParseNullableEnum<EmailContentType>(
                source.ReminderEmailContentType?.ToString()
            ),
            ReminderSmsBody = source.ReminderSmsBody,
            NotificationChannel = ParseEnum(
                source.NotificationChannel?.ToString(),
                NotificationChannel.Email
            ),
            ReminderNotificationChannel = ParseNullableEnum<NotificationChannel>(
                source.ReminderNotificationChannel?.ToString()
            ),
            SendersReference = source.SendersReference,
            CustomRecipients =
            [
                .. (source.CustomRecipients ?? []).Select(recipient => new NotificationRecipient
                {
                    EmailAddress = recipient.EmailAddress,
                    MobileNumber = recipient.MobileNumber,
                    OrganizationNumber = recipient.OrganizationNumber,
                    NationalIdentityNumber = recipient.NationalIdentityNumber,
                    IsReserved = recipient.IsReserved,
                }),
            ],
            OverrideRegisteredContactInformation =
                source.OverrideRegisteredContactInformation ?? false,
        };
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value, out var parsed) ? parsed : fallback;

    private static TEnum? ParseNullableEnum<TEnum>(string? value)
        where TEnum : struct, Enum => Enum.TryParse<TEnum>(value, out var parsed) ? parsed : null;

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
