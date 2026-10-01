using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Implementation.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Microsoft.AspNetCore.Http;
using Shouldly;

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
    public async Task MinimalCorrespondenceRequest_Maps_ToMultipartBody()
    {
        var request = CreateMinimalCorrespondenceRequest().ToApiRequest();

        var formFields = await ExtractFormFields(request);

        await Verifier.Verify(formFields, _verifySettings);
    }

    [Fact]
    public async Task FullCorrespondenceRequest_Maps_ToMultipartBody()
    {
        var request = CreateFullCorrespondenceRequest().ToApiRequest();

        var formFields = await ExtractFormFields(request);

        await Verifier.Verify(formFields, _verifySettings);
    }

    [Fact]
    public async Task MultipleAttachments_Map_ToRepeatedFileParts()
    {
        var request = CreateMinimalCorrespondenceRequest().ToApiRequest();
        List<IFormFile> attachments =
        [
            CreateFormFile("first", "first.txt"),
            CreateFormFile("second", "second.txt"),
        ];

        var form = await ReadForm(request, attachments);
        form.Files.Select(file => file.Name).ShouldBe(["attachments", "attachments"]);
        form.Files.Select(file => file.FileName).ShouldBe(["first.txt", "second.txt"]);
        using var first = new StreamReader(form.Files[0].OpenReadStream());
        first.ReadToEnd().ShouldBe("first");
        using var second = new StreamReader(form.Files[1].OpenReadStream());
        second.ReadToEnd().ShouldBe("second");
    }

    private static FormFile CreateFormFile(string content, string fileName) =>
        new(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)),
            0,
            content.Length,
            "attachments",
            fileName
        )
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain",
        };

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
    private static async Task<Dictionary<string, string>> ExtractFormFields(
        InitializeCorrespondences request,
        List<IFormFile>? attachments = null
    ) =>
        (await ReadForm(request, attachments)).ToDictionary(
            field => field.Key,
            field => field.Value.ToString()
        );

    private static async Task<IFormCollection> ReadForm(
        InitializeCorrespondences request,
        List<IFormFile>? attachments
    )
    {
        using var body = request.ToMultipartFormDataContent(attachments);
        var context = new DefaultHttpContext();
        context.Request.ContentType = body.Headers.ContentType!.ToString();
        context.Request.Body = new MemoryStream(
            await body.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)
        );
        return await context.Request.ReadFormAsync(TestContext.Current.CancellationToken);
    }
}
