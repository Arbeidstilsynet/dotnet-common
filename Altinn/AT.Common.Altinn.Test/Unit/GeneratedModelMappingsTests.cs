using Arbeidstilsynet.Common.Altinn.Implementation.Mapping;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Shouldly;
using GeneratedApps = Arbeidstilsynet.Common.Altinn.Apps.Models;
using GeneratedCorrespondence = Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using GeneratedEvents = Arbeidstilsynet.Common.Altinn.Events.Models;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class GeneratedModelMappingsTests
{
    [Fact]
    public void AppsInstance_PreservesDataAndDataValues()
    {
        var source = new GeneratedApps.Instance
        {
            Data =
            [
                new GeneratedApps.DataElement
                {
                    Id = "data-id",
                    DataType = "model",
                    Filename = "data.xml",
                    Tags = ["signed"],
                    Metadata = [new GeneratedApps.KeyValueEntry { Key = "kind", Value = "form" }],
                },
            ],
            DataValues = new GeneratedApps.Instance_dataValues
            {
                AdditionalData = new Dictionary<string, object> { ["state"] = "submitted" },
            },
        };

        var result = source.ToAltinnInstance();

        result.Data.ShouldHaveSingleItem().Id.ShouldBe("data-id");
        result.Data[0].Metadata["kind"].ShouldBe("form");
        result.DataValues["state"].ShouldBe("submitted");
    }

    [Fact]
    public void JsonCorrespondenceRequest_PreservesNotification()
    {
        var source = new InitializeCorrespondences
        {
            Correspondence = new BaseCorrespondence
            {
                ResourceId = "resource",
                SendersReference = "reference",
                Notification = new InitializeCorrespondenceNotification
                {
                    NotificationTemplate = NotificationTemplate.CustomMessage,
                    NotificationChannel = NotificationChannel.EmailAndSms,
                    EmailSubject = "Subject",
                    CustomRecipients =
                    [
                        new NotificationRecipient { EmailAddress = "recipient@example.com" },
                    ],
                },
            },
            Recipients = ["urn:altinn:organization:identifier-no:123456789"],
        };

        var result = source.ToGenerated();

        result.Correspondence!.Notification!.EmailSubject.ShouldBe("Subject");
        result.Correspondence.Notification.NotificationChannel.ShouldBe(
            GeneratedCorrespondence.NotificationChannelExt.EmailAndSms
        );
        result
            .Correspondence.Notification.CustomRecipients.ShouldHaveSingleItem()
            .EmailAddress.ShouldBe("recipient@example.com");
    }

    [Fact]
    public void CorrespondenceOverview_PreservesNestedFields()
    {
        var notificationOrderId = Guid.NewGuid();
        var source = new GeneratedCorrespondence.CorrespondenceOverviewExt
        {
            ResourceId = "resource",
            SendersReference = "reference",
            Recipient = "recipient",
            Content = new GeneratedCorrespondence.CorrespondenceContentExt
            {
                MessageTitle = "Title",
                MessageBody = "Body",
                Attachments =
                [
                    new GeneratedCorrespondence.CorrespondenceAttachmentExt
                    {
                        Id = Guid.NewGuid(),
                        SendersReference = "attachment",
                    },
                ],
            },
            ExternalReferences =
            [
                new GeneratedCorrespondence.ExternalReferenceExt
                {
                    ReferenceType = GeneratedCorrespondence.ReferenceTypeExt.AltinnAppInstance,
                    ReferenceValue = "instance",
                },
            ],
            ReplyOptions =
            [
                new GeneratedCorrespondence.CorrespondenceReplyOptionExt
                {
                    LinkURL = "https://example.com",
                },
            ],
            Notification = new GeneratedCorrespondence.InitializeCorrespondenceNotificationExt
            {
                EmailSubject = "Subject",
            },
            Notifications =
            [
                new GeneratedCorrespondence.CorrespondenceNotificationOverviewExt
                {
                    NotificationOrderId = notificationOrderId,
                    IsReminder = true,
                },
            ],
        };

        var result = source.ToOverview();

        result.Content!.MessageTitle.ShouldBe("Title");
        result.Content.Attachments.ShouldHaveSingleItem().SendersReference.ShouldBe("attachment");
        result.ExternalReferences.ShouldHaveSingleItem().ReferenceValue.ShouldBe("instance");
        result.ReplyOptions.ShouldHaveSingleItem().LinkURL.ShouldBe("https://example.com");
        result.Notification!.EmailSubject.ShouldBe("Subject");
        result
            .Notifications.ShouldHaveSingleItem()
            .NotificationOrderId.ShouldBe(notificationOrderId);
    }

    [Fact]
    public void CorrespondenceInitializationResponse_PreservesNotifications()
    {
        var orderId = Guid.NewGuid();
        var source = new GeneratedCorrespondence.InitializeCorrespondencesResponseExt
        {
            Correspondences =
            [
                new GeneratedCorrespondence.InitializedCorrespondencesExt
                {
                    Recipient = "recipient",
                    Notifications =
                    [
                        new GeneratedCorrespondence.InitializedCorrespondencesNotificationsExt
                        {
                            OrderId = orderId,
                            IsReminder = true,
                            Status = GeneratedCorrespondence
                                .InitializedNotificationStatusExt
                                .Success,
                        },
                    ],
                },
            ],
        };

        var result = source.ToResponse();

        var notification = result
            .Correspondences.ShouldHaveSingleItem()
            .Notifications.ShouldHaveSingleItem();
        notification.OrderId.ShouldBe(orderId);
        notification.IsReminder.ShouldBe(true);
        notification.Status.ShouldBe(InitializedNotificationStatus.Success);
    }

    [Fact]
    public void EventSubscription_PreservesAlternativeSubjectFilter()
    {
        var source = new GeneratedEvents.Subscription { AlternativeSubjectFilter = "/party/123" };

        source.ToAltinnSubscription().AlternativeSubjectFilter.ShouldBe("/party/123");
    }
}
