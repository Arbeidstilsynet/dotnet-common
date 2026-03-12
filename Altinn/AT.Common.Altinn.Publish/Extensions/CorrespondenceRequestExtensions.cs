using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Extensions;

internal static class CorrespondenceRequestExtensions
{
    /// <summary>
    /// Converts a <see cref="CorrespondenceRequest"/> and its attachments into a
    /// <see cref="MultipartFormDataContent"/> that conforms to the Altinn Correspondence
    /// upload endpoint's [FromForm] binding contract.
    /// </summary>
    public static MultipartFormDataContent ToMultipartFormData(
        this CorrespondenceRequest request,
        List<IFormFile>? attachments
    )
    {
        var formData = new MultipartFormDataContent();

        AddTopLevelFields(formData, request);
        AddCorrespondenceFields(formData, request);

        if (request.Content is not null)
        {
            AddContent(formData, request.Content);
        }

        if (request.Notification is not null)
        {
            AddNotification(formData, request.Notification);
        }

        AddCollections(formData, request);
        if (attachments != null)
            AddFileAttachments(formData, attachments);

        return formData;
    }

    private static void AddTopLevelFields(
        MultipartFormDataContent formData,
        CorrespondenceRequest request
    )
    {
        for (var i = 0; i < request.Recipients.Count; i++)
        {
            formData.Add(new StringContent(request.Recipients[i]), $"recipients[{i}]");
        }

        if (request.ExistingAttachments is not null)
        {
            for (var i = 0; i < request.ExistingAttachments.Count; i++)
            {
                formData.Add(
                    new StringContent(request.ExistingAttachments[i].ToString()),
                    $"existingAttachments[{i}]"
                );
            }
        }

        if (request.IdempotentKey.HasValue)
        {
            formData.Add(
                new StringContent(request.IdempotentKey.Value.ToString()),
                "idempotentKey"
            );
        }
    }

    private static void AddCorrespondenceFields(
        MultipartFormDataContent formData,
        CorrespondenceRequest request
    )
    {
        formData.Add(new StringContent(request.ResourceId), "correspondence.resourceId");
        formData.Add(
            new StringContent(request.SendersReference),
            "correspondence.sendersReference"
        );

        if (request.MessageSender is not null)
        {
            formData.Add(new StringContent(request.MessageSender), "correspondence.messageSender");
        }

        if (request.RequestedPublishTime.HasValue)
        {
            formData.Add(
                new StringContent(request.RequestedPublishTime.Value.ToString("O")),
                "correspondence.requestedPublishTime"
            );
        }

        if (request.DueDateTime.HasValue)
        {
            formData.Add(
                new StringContent(request.DueDateTime.Value.ToString("O")),
                "correspondence.dueDateTime"
            );
        }

        if (request.IgnoreReservation.HasValue)
        {
            formData.Add(
                new StringContent(request.IgnoreReservation.Value.ToString()),
                "correspondence.ignoreReservation"
            );
        }

        formData.Add(
            new StringContent(request.IsConfirmationNeeded.ToString()),
            "correspondence.isConfirmationNeeded"
        );

        formData.Add(
            new StringContent(request.IsConfidential.ToString()),
            "correspondence.isConfidential"
        );
    }

    private static void AddCollections(
        MultipartFormDataContent formData,
        CorrespondenceRequest request
    )
    {
        if (request.ExternalReferences is not null)
        {
            for (var i = 0; i < request.ExternalReferences.Count; i++)
            {
                var r = request.ExternalReferences[i];
                formData.Add(
                    new StringContent(r.ReferenceType.ToString()),
                    $"correspondence.externalReferences[{i}].referenceType"
                );
                formData.Add(
                    new StringContent(r.ReferenceValue),
                    $"correspondence.externalReferences[{i}].referenceValue"
                );
            }
        }

        if (request.ReplyOptions is not null)
        {
            for (var i = 0; i < request.ReplyOptions.Count; i++)
            {
                var opt = request.ReplyOptions[i];
                formData.Add(
                    new StringContent(opt.LinkURL),
                    $"correspondence.replyOptions[{i}].linkURL"
                );
                if (opt.LinkText is not null)
                {
                    formData.Add(
                        new StringContent(opt.LinkText),
                        $"correspondence.replyOptions[{i}].linkText"
                    );
                }
            }
        }

        foreach (var kvp in request.PropertyList)
        {
            formData.Add(new StringContent(kvp.Value), $"correspondence.propertyList.{kvp.Key}");
        }
    }

    private static void AddFileAttachments(
        MultipartFormDataContent formData,
        List<IFormFile> attachments
    )
    {
        foreach (var attachment in attachments)
        {
            var stream = attachment.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                attachment.ContentType ?? "application/octet-stream"
            );
            formData.Add(fileContent, "attachments", attachment.FileName);
        }
    }

    private static void AddContent(
        MultipartFormDataContent formData,
        InitializeCorrespondenceContent content
    )
    {
        formData.Add(
            new StringContent(content.MessageTitle),
            "correspondence.content.messageTitle"
        );
        formData.Add(new StringContent(content.MessageBody), "correspondence.content.messageBody");

        if (content.Language is not null)
        {
            formData.Add(new StringContent(content.Language), "correspondence.content.language");
        }

        if (content.MessageSummary is not null)
        {
            formData.Add(
                new StringContent(content.MessageSummary),
                "correspondence.content.messageSummary"
            );
        }

        for (var i = 0; i < content.Attachments.Count; i++)
        {
            var att = content.Attachments[i];
            formData.Add(
                new StringContent(att.DataLocationType.ToString()),
                $"correspondence.content.attachments[{i}].dataLocationType"
            );
            formData.Add(
                new StringContent(att.SendersReference),
                $"correspondence.content.attachments[{i}].sendersReference"
            );
            formData.Add(
                new StringContent(att.IsEncrypted.ToString()),
                $"correspondence.content.attachments[{i}].isEncrypted"
            );

            if (att.FileName is not null)
            {
                formData.Add(
                    new StringContent(att.FileName),
                    $"correspondence.content.attachments[{i}].fileName"
                );
            }

            if (att.DisplayName is not null)
            {
                formData.Add(
                    new StringContent(att.DisplayName),
                    $"correspondence.content.attachments[{i}].displayName"
                );
            }

            if (att.Checksum is not null)
            {
                formData.Add(
                    new StringContent(att.Checksum),
                    $"correspondence.content.attachments[{i}].checksum"
                );
            }

            if (att.ExpirationInDays.HasValue)
            {
                formData.Add(
                    new StringContent(att.ExpirationInDays.Value.ToString()),
                    $"correspondence.content.attachments[{i}].expirationInDays"
                );
            }
        }
    }

    private static void AddNotification(
        MultipartFormDataContent formData,
        InitializeCorrespondenceNotification notification
    )
    {
        if (notification.NotificationTemplate.HasValue)
        {
            formData.Add(
                new StringContent(notification.NotificationTemplate.Value.ToString()),
                "correspondence.notification.notificationTemplate"
            );
        }

        formData.Add(
            new StringContent(notification.SendReminder.ToString()),
            "correspondence.notification.sendReminder"
        );

        formData.Add(
            new StringContent(notification.NotificationChannel.ToString()),
            "correspondence.notification.notificationChannel"
        );

        formData.Add(
            new StringContent(notification.EmailContentType.ToString()),
            "correspondence.notification.emailContentType"
        );

        if (notification.EmailSubject is not null)
        {
            formData.Add(
                new StringContent(notification.EmailSubject),
                "correspondence.notification.emailSubject"
            );
        }

        if (notification.EmailBody is not null)
        {
            formData.Add(
                new StringContent(notification.EmailBody),
                "correspondence.notification.emailBody"
            );
        }

        if (notification.SmsBody is not null)
        {
            formData.Add(
                new StringContent(notification.SmsBody),
                "correspondence.notification.smsBody"
            );
        }

        if (notification.ReminderEmailSubject is not null)
        {
            formData.Add(
                new StringContent(notification.ReminderEmailSubject),
                "correspondence.notification.reminderEmailSubject"
            );
        }

        if (notification.ReminderEmailBody is not null)
        {
            formData.Add(
                new StringContent(notification.ReminderEmailBody),
                "correspondence.notification.reminderEmailBody"
            );
        }

        if (notification.ReminderEmailContentType.HasValue)
        {
            formData.Add(
                new StringContent(notification.ReminderEmailContentType.Value.ToString()),
                "correspondence.notification.reminderEmailContentType"
            );
        }

        if (notification.ReminderSmsBody is not null)
        {
            formData.Add(
                new StringContent(notification.ReminderSmsBody),
                "correspondence.notification.reminderSmsBody"
            );
        }

        if (notification.ReminderNotificationChannel.HasValue)
        {
            formData.Add(
                new StringContent(notification.ReminderNotificationChannel.Value.ToString()),
                "correspondence.notification.reminderNotificationChannel"
            );
        }

        if (notification.SendersReference is not null)
        {
            formData.Add(
                new StringContent(notification.SendersReference),
                "correspondence.notification.sendersReference"
            );
        }

        formData.Add(
            new StringContent(notification.OverrideRegisteredContactInformation.ToString()),
            "correspondence.notification.overrideRegisteredContactInformation"
        );

        if (notification.CustomRecipients is not null)
        {
            for (var i = 0; i < notification.CustomRecipients.Count; i++)
            {
                AddNotificationRecipient(
                    formData,
                    notification.CustomRecipients[i],
                    $"correspondence.notification.customRecipients[{i}]"
                );
            }
        }
    }

    private static void AddNotificationRecipient(
        MultipartFormDataContent formData,
        NotificationRecipient recipient,
        string prefix
    )
    {
        if (recipient.EmailAddress is not null)
        {
            formData.Add(new StringContent(recipient.EmailAddress), $"{prefix}.emailAddress");
        }

        if (recipient.MobileNumber is not null)
        {
            formData.Add(new StringContent(recipient.MobileNumber), $"{prefix}.mobileNumber");
        }

        if (recipient.OrganizationNumber is not null)
        {
            formData.Add(
                new StringContent(recipient.OrganizationNumber),
                $"{prefix}.organizationNumber"
            );
        }

        if (recipient.NationalIdentityNumber is not null)
        {
            formData.Add(
                new StringContent(recipient.NationalIdentityNumber),
                $"{prefix}.nationalIdentityNumber"
            );
        }

        if (recipient.IsReserved.HasValue)
        {
            formData.Add(
                new StringContent(recipient.IsReserved.Value.ToString()),
                $"{prefix}.isReserved"
            );
        }
    }
}
