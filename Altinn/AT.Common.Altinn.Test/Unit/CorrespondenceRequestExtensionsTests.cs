using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;

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
            Content = new InitializeCorrespondenceContent
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
            Content = new InitializeCorrespondenceContent
            {
                Language = "nb",
                MessageTitle = "Full Test Title",
                MessageSummary = "A summary of the correspondence",
                MessageBody = "Full test body with details",
                Attachments =
                [
                    new InitializeCorrespondenceAttachment
                    {
                        DataLocationType =
                            InitializeAttachmentDataLocationType.NewCorrespondenceAttachment,
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
                new ExternalReference
                {
                    ReferenceType = ReferenceType.AltinnAppInstance,
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
                new CorrespondenceReplyOption
                {
                    LinkURL = "https://example.com/reply",
                    LinkText = "Reply here",
                },
            ],
            Notification = new InitializeCorrespondenceNotification
            {
                NotificationTemplate = NotificationTemplate.CustomMessage,
                NotificationChannel = NotificationChannel.EmailPreferred,
                EmailContentType = EmailContentType.Html,
                EmailSubject = "You have a new correspondence",
                EmailBody = "<p>Please check your inbox</p>",
                SmsBody = "New correspondence available",
                SendReminder = true,
                ReminderEmailSubject = "Reminder: Unread correspondence",
                ReminderEmailBody = "<p>Reminder: please read</p>",
                ReminderEmailContentType = EmailContentType.Html,
                ReminderSmsBody = "Reminder: check your inbox",
                ReminderNotificationChannel = NotificationChannel.EmailAndSms,
                SendersReference = "NOTIF-REF-001",
                OverrideRegisteredContactInformation = true,
                CustomRecipients =
                [
                    new NotificationRecipient
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
    public async Task MinimalCorrespondenceRequest_Maps_ToMultipartFormData()
    {
        var request = CreateMinimalCorrespondenceRequest().ToApiRequest();

        var result = request.ToMultipartFormData(attachments: null);
        var formFields = await ExtractFormFields(result);

        await Verifier.Verify(formFields, _verifySettings);
    }

    [Fact]
    public async Task FullCorrespondenceRequest_Maps_ToMultipartFormData()
    {
        var request = CreateFullCorrespondenceRequest().ToApiRequest();

        var result = request.ToMultipartFormData(attachments: null);
        var formFields = await ExtractFormFields(result);

        await Verifier.Verify(formFields, _verifySettings);
    }

    [Fact]
    public async Task ReceiverTypes_Map_ToReceiverList()
    {
        List<IReceiver> receivers =
        [
            new Organization { OrgNumber = "123456789" },
            new NorwegianCitizen { SosialSecurityNumber = "12345678901" },
            new SelfRegisteredUser { EmailAddress = "user@example.com" },
        ];

        var result = receivers.ToReceiverList();

        await Verifier.Verify(result, _verifySettings);
    }

    private static async Task<Dictionary<string, string>> ExtractFormFields(
        MultipartFormDataContent formData
    )
    {
        var fields = new Dictionary<string, string>();
        foreach (var content in formData)
        {
            var name = content.Headers.ContentDisposition?.Name?.Trim('"') ?? "unknown";
            var value = await content.ReadAsStringAsync();
            fields[name] = value;
        }
        return fields;
    }
}
