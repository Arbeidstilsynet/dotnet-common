using System.Net.Http.Headers;
using System.Text;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Microsoft.AspNetCore.Http;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Extensions;

/// <summary>
/// Flattens a correspondence request into the form fields the upload endpoint binds against.
/// </summary>
/// <remarks>
/// <para>
/// The upload endpoint accepts only multipart/form-data, and its schema describes 44 flat,
/// dot-separated fields rather than a nested object -- it mirrors an ASP.NET Core
/// <c>[FromForm]</c> binding contract. Kiota generates no model for such a schema, so the
/// generated builder takes a bare <c>MultipartBody</c> and the flattening has to live here.
/// </para>
/// <para>
/// File parts repeat the name "attachments"; indexing it prevents ASP.NET's file binder from
/// finding them. MultipartFormDataContent also preserves files with identical names, whereas
/// Kiota's MultipartBody replaces parts sharing a field name and file name.
/// </para>
/// </remarks>
internal static class CorrespondenceMultipartExtensions
{
    private const string TextContentType = "text/plain";
    private const string DefaultAttachmentContentType = "application/octet-stream";

    public static MultipartFormDataContent ToMultipartFormDataContent(
        this InitializeCorrespondences request,
        List<IFormFile>? attachments
    )
    {
        var body = new MultipartFormDataContent();

        body.AddCollection("Recipients", request.Recipients);
        body.AddCollection("ExistingAttachments", request.ExistingAttachments);
        body.AddText("IdempotentKey", request.IdempotentKey);

        if (request.Correspondence is { } correspondence)
        {
            body.AddCorrespondence(correspondence);
        }

        var files = attachments ?? [];

        for (var i = 0; i < files.Count; i++)
        {
            var attachment = files[i];
            var contentType = new MediaTypeHeaderValue(
                attachment.ContentType ?? DefaultAttachmentContentType
            );
            var content = new StreamContent(attachment.OpenReadStream());
            content.Headers.ContentType = contentType;
            body.Add(content, "attachments", attachment.FileName);
        }

        return body;
    }

    private static void AddCorrespondence(
        this MultipartFormDataContent body,
        BaseCorrespondence source
    )
    {
        const string prefix = "Correspondence";

        body.AddText($"{prefix}.ResourceId", source.ResourceId);
        body.AddText($"{prefix}.SendersReference", source.SendersReference);
        body.AddText($"{prefix}.MessageSender", source.MessageSender);
        body.AddText($"{prefix}.RequestedPublishTime", source.RequestedPublishTime);
        body.AddText($"{prefix}.DueDateTime", source.DueDateTime);
        body.AddText($"{prefix}.IgnoreReservation", source.IgnoreReservation);
        body.AddText($"{prefix}.IsConfirmationNeeded", source.IsConfirmationNeeded);
        body.AddText($"{prefix}.IsConfidential", source.IsConfidential);

        if (source.Content is { } content)
        {
            body.AddContent($"{prefix}.Content", content);
        }

        if (source.Notification is { } notification)
        {
            body.AddNotification($"{prefix}.Notification", notification);
        }

        var externalReferences = source.ExternalReferences ?? [];

        for (var i = 0; i < externalReferences.Count; i++)
        {
            var reference = externalReferences[i];
            body.AddText(
                $"{prefix}.ExternalReferences[{i}].ReferenceType",
                reference.ReferenceType
            );
            body.AddText(
                $"{prefix}.ExternalReferences[{i}].ReferenceValue",
                reference.ReferenceValue
            );
        }

        var replyOptions = source.ReplyOptions ?? [];

        for (var i = 0; i < replyOptions.Count; i++)
        {
            var replyOption = replyOptions[i];
            body.AddText($"{prefix}.ReplyOptions[{i}].LinkURL", replyOption.LinkURL);
            body.AddText($"{prefix}.ReplyOptions[{i}].LinkText", replyOption.LinkText);
        }

        var propertyList = source.PropertyList;

        if (propertyList is not null)
        {
            foreach (var property in propertyList)
            {
                body.AddText($"{prefix}.PropertyList[{property.Key}]", property.Value);
            }
        }
    }

    private static void AddContent(
        this MultipartFormDataContent body,
        string prefix,
        InitializeCorrespondenceContent content
    )
    {
        body.AddText($"{prefix}.Language", content.Language);
        body.AddText($"{prefix}.MessageTitle", content.MessageTitle);
        body.AddText($"{prefix}.MessageSummary", content.MessageSummary);
        body.AddText($"{prefix}.MessageBody", content.MessageBody);

        var attachments = content.Attachments ?? [];

        for (var i = 0; i < attachments.Count; i++)
        {
            var attachment = attachments[i];
            var attachmentPrefix = $"{prefix}.Attachments[{i}]";

            body.AddText($"{attachmentPrefix}.DataLocationType", attachment.DataLocationType);
            body.AddText($"{attachmentPrefix}.SendersReference", attachment.SendersReference);
            body.AddText($"{attachmentPrefix}.IsEncrypted", attachment.IsEncrypted);
            body.AddText($"{attachmentPrefix}.FileName", attachment.FileName);
            body.AddText($"{attachmentPrefix}.DisplayName", attachment.DisplayName);
            body.AddText($"{attachmentPrefix}.Checksum", attachment.Checksum);
            body.AddText($"{attachmentPrefix}.ExpirationInDays", attachment.ExpirationInDays);
        }
    }

    private static void AddNotification(
        this MultipartFormDataContent body,
        string prefix,
        InitializeCorrespondenceNotification notification
    )
    {
        body.AddText($"{prefix}.NotificationTemplate", notification.NotificationTemplate);
        body.AddText($"{prefix}.NotificationChannel", notification.NotificationChannel);
        body.AddText($"{prefix}.SendReminder", notification.SendReminder);
        body.AddText($"{prefix}.SendersReference", notification.SendersReference);

        body.AddText($"{prefix}.EmailSubject", notification.EmailSubject);
        body.AddText($"{prefix}.EmailBody", notification.EmailBody);
        body.AddText($"{prefix}.EmailContentType", notification.EmailContentType);
        body.AddText($"{prefix}.SmsBody", notification.SmsBody);

        body.AddText($"{prefix}.ReminderEmailSubject", notification.ReminderEmailSubject);
        body.AddText($"{prefix}.ReminderEmailBody", notification.ReminderEmailBody);
        body.AddText($"{prefix}.ReminderEmailContentType", notification.ReminderEmailContentType);
        body.AddText($"{prefix}.ReminderSmsBody", notification.ReminderSmsBody);
        body.AddText(
            $"{prefix}.ReminderNotificationChannel",
            notification.ReminderNotificationChannel
        );

        body.AddText(
            $"{prefix}.OverrideRegisteredContactInformation",
            notification.OverrideRegisteredContactInformation
        );
    }

    private static void AddCollection<T>(
        this MultipartFormDataContent body,
        string name,
        List<T>? values
    )
    {
        var items = values ?? [];

        for (var i = 0; i < items.Count; i++)
        {
            body.AddText($"{name}[{i}]", items[i]);
        }
    }

    /// <summary>
    /// Adds a text part, skipping values the caller left unset so that the server applies its own
    /// defaults rather than receiving an empty string.
    /// </summary>
    private static void AddText<T>(this MultipartFormDataContent body, string name, T? value)
    {
        var text = value switch
        {
            null => null,
            bool boolean => boolean ? "true" : "false",
            DateTimeOffset timestamp => timestamp.ToString("O"),
            _ => value.ToString(),
        };

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        body.Add(new StringContent(text, Encoding.UTF8, TextContentType), name);
    }
}
