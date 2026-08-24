using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;
using Microsoft.Kiota.Serialization.Multipart;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class CorrespondenceRequestExtensionsTests
{
    private readonly VerifySettings _verifySettings = new();

    public CorrespondenceRequestExtensionsTests()
    {
        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    private static CorrespondenceRequest CreateMinimalCorrespondenceRequest() =>
        new()
        {
            SendersReference = "REF-001",
            Content = new InitializeCorrespondenceContentExt
            {
                MessageTitle = "Test Title",
                MessageBody = "Test Body",
            },
            Recipients = [new Organization { OrgNumber = "123456789" }],
        };

    private static CorrespondenceRequest CreateFullCorrespondenceRequest() =>
        new()
        {
            ResourceIdentifier = "dat-tilsyn-correspondence",
            SendersReference = "REF-002",
            MessageSender = "Arbeidstilsynet",
            Content = new InitializeCorrespondenceContentExt
            {
                Language = "nb",
                MessageTitle = "Full Test Title",
                MessageSummary = "A summary of the correspondence",
                MessageBody = "Full test body with details",
                Attachments =
                [
                    new InitializeCorrespondenceAttachmentExt
                    {
                        DataLocationType =
                            InitializeAttachmentDataLocationTypeExt.NewCorrespondenceAttachment,
                        SendersReference = "ATT-REF-001",
                        FileName = "report.pdf",
                        DisplayName = "Inspection Report",
                        IsEncrypted = false,
                        Checksum = "d41d8cd98f00b204e9800998ecf8427e",
                        ExpirationInDays = 30,
                    },
                ],
            },
            RequestedPublishTime = DateTimeOffset.UtcNow,
            DueDateTime = DateTimeOffset.UtcNow.AddDays(30),
            ExternalReferences =
            [
                new ExternalReferenceExt
                {
                    ReferenceType = ReferenceTypeExt.AltinnAppInstance,
                    ReferenceValue = "instance-ref-123",
                },
            ],
            PropertyList = new Dictionary<string, string>
            {
                { "caseId", "CASE-42" },
                { "priority", "high" },
            },
            ReplyOptions =
            [
                new CorrespondenceReplyOptionExt
                {
                    LinkURL = "https://example.com/reply",
                    LinkText = "Reply here",
                },
            ],
            Notification = new InitializeCorrespondenceNotificationExt
            {
                NotificationTemplate = NotificationTemplateExt.CustomMessage,
                NotificationChannel = NotificationChannelExt.EmailPreferred,
                EmailContentType = EmailContentType.Html,
                EmailSubject = "You have a new correspondence",
                EmailBody = "<p>Please check your inbox</p>",
                SmsBody = "New correspondence available",
                SendReminder = true,
                ReminderEmailSubject = "Reminder: Unread correspondence",
                ReminderEmailBody = "<p>Reminder: please read</p>",
                ReminderEmailContentType = EmailContentType.Html,
                ReminderSmsBody = "Reminder: check your inbox",
                ReminderNotificationChannel = NotificationChannelExt.EmailAndSms,
                SendersReference = "NOTIF-REF-001",
                OverrideRegisteredContactInformation = true,
                CustomRecipients =
                [
                    new NotificationRecipientExt
                    {
                        EmailAddress = "test@example.com",
                        MobileNumber = "+4799887766",
                        OrganizationNumber = "987654321",
                        NationalIdentityNumber = "12345678901",
                        IsReserved = false,
                    },
                ],
            },
            IgnoreReservation = true,
            IsConfirmationNeeded = true,
            IsConfidential = true,
            ExistingAttachments = [Guid.NewGuid()],
            IdempotentKey = Guid.NewGuid(),
            Recipients =
            [
                new Organization { OrgNumber = "123456789" },
                new NorwegianCitizen { SosialSecurityNumber = "12345678901" },
                new SelfRegisteredUser { EmailAddress = "user@example.com" },
            ],
        };

    [Fact]
    public async Task MinimalCorrespondenceRequest_Maps_ToApiRequest()
    {
        var request = CreateMinimalCorrespondenceRequest();

        var result = request.ToApiRequest();

        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task FullCorrespondenceRequest_Maps_ToApiRequest()
    {
        var request = CreateFullCorrespondenceRequest();

        var result = request.ToApiRequest();

        await Verifier.Verify(result, _verifySettings);
    }

    [Fact]
    public async Task MinimalCorrespondenceRequest_Maps_ToMultipartBody()
    {
        var request = CreateMinimalCorrespondenceRequest().ToApiRequest();

        var formFields = ExtractFormFields(request);

        await Verifier.Verify(formFields, _verifySettings);
    }

    [Fact]
    public async Task FullCorrespondenceRequest_Maps_ToMultipartBody()
    {
        var request = CreateFullCorrespondenceRequest().ToApiRequest();

        var formFields = ExtractFormFields(request);

        await Verifier.Verify(formFields, _verifySettings);
    }

    [Fact]
    public async Task ReceiverTypes_Map_ToReceiverList()
    {
        List<IAltinnRecipient> receivers =
        [
            new Organization { OrgNumber = "123456789" },
            new NorwegianCitizen { SosialSecurityNumber = "12345678901" },
            new SelfRegisteredUser { EmailAddress = "user@example.com" },
        ];

        var result = receivers.ToReceiverList();

        await Verifier.Verify(result, _verifySettings);
    }

    /// <summary>
    /// Serialises the request as the generated client would and returns the resulting form fields,
    /// so the snapshot covers the exact names and values that reach the wire.
    /// </summary>
    private static Dictionary<string, string> ExtractFormFields(
        InitializeCorrespondencesExt request
    )
    {
        var adapter = new DefaultRequestAdapter(new AnonymousAuthenticationProvider());

        var body = request.ToMultipartBody(adapter, attachments: null);

        var writer = new MultipartSerializationWriterFactory().GetSerializationWriter(
            "multipart/form-data"
        );
        writer.WriteObjectValue(string.Empty, body);

        using var stream = writer.GetSerializedContent();
        using var reader = new StreamReader(stream);

        var fields = new Dictionary<string, string>();

        foreach (
            var part in reader
                .ReadToEnd()
                .Split($"--{body.Boundary}", StringSplitOptions.RemoveEmptyEntries)
        )
        {
            var nameStart = part.IndexOf("name=\"", StringComparison.Ordinal);

            if (nameStart < 0)
            {
                continue;
            }

            nameStart += "name=\"".Length;
            var name = part[nameStart..part.IndexOf('"', nameStart)];

            var headerEnd = part.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            var separatorLength = 4;

            if (headerEnd < 0)
            {
                headerEnd = part.IndexOf("\n\n", StringComparison.Ordinal);
                separatorLength = 2;
            }

            var value = headerEnd < 0 ? string.Empty : part[(headerEnd + separatorLength)..].Trim();

            fields[name] = value;
        }

        return fields;
    }
}
