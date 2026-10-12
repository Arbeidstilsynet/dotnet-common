using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Extensions;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Ports.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Arbeidstilsynet.Common.Altinn.Ports.Token;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Kiota.Abstractions;
using NSubstitute;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

public class CorrespondenceUploadTests
{
    private const string FingerprintKey = "kommunikasjon.submission-fingerprint.v1";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(10, false)]
    [InlineData(2, true)]
    public async Task Upload_BindsEveryFileIncludingRepeatedNames(int count, bool sameName)
    {
        var files = Enumerable
            .Range(0, count)
            .Select(index => File(sameName ? "same.pdf" : $"synthetic-{index}.pdf", index))
            .ToList();
        var handler = new CaptureHandler();
        await using var services = Services(handler);
        await services
            .GetRequiredService<IAltinnMeldingerAdapter>()
            .CreateCorrespondence(Request(files), files);

        handler.Path.ShouldBe("/correspondence/api/v1/correspondence/upload");
        handler.MediaType.ShouldStartWith("multipart/form-data; boundary=");
        var context = Context(handler, services);
        var form = await context.Request.ReadFormAsync(Token);
        form.Files.Count.ShouldBe(count);
        var binding = Binding(
            context,
            new EmptyModelMetadataProvider().GetMetadataForType(typeof(List<IFormFile>)),
            "attachments"
        );
        await new FormFileModelBinder(NullLoggerFactory.Instance).BindModelAsync(binding);
        var bound = binding.Result.Model.ShouldBeOfType<List<IFormFile>>();
        bound.Count.ShouldBe(count);
        for (var i = 0; i < count; i++)
        {
            bound[i].Name.ShouldBe("attachments");
            bound[i].FileName.ShouldBe(files[i].FileName);
            bound[i].ContentType.ShouldBe("application/pdf");
            form[$"Correspondence.Content.Attachments[{i}].FileName"]
                .ToString()
                .ShouldBe(files[i].FileName);
            form[$"Correspondence.Content.Attachments[{i}].DisplayName"]
                .ToString()
                .ShouldBe($"Display {i}");
            await using var stream = bound[i].OpenReadStream();
            using var bytes = new MemoryStream();
            await stream.CopyToAsync(bytes, Token);
            bytes.ToArray().ShouldBe(Pdf(i));
        }
        handler.Scopes.ShouldBe("altinn:serviceowner altinn:correspondence.write");
    }

    [Fact]
    public async Task Upload_BindsDottedPropertyKeysWithoutTruncationOrNullValues()
    {
        List<IFormFile> files = [File("synthetic.pdf")];
        var handler = new CaptureHandler();
        await using var services = Services(handler);
        var request = Request(files);
        await services
            .GetRequiredService<IAltinnMeldingerAdapter>()
            .CreateCorrespondence(request, files);
        var context = Context(handler, services);
        var form = await context.Request.ReadFormAsync(Token);
        var metadata = services
            .GetRequiredService<IModelMetadataProvider>()
            .GetMetadataForType(typeof(Dictionary<string, string>));
        var binding = Binding(context, metadata, "Correspondence.PropertyList");
        binding.ValueProvider = new FormValueProvider(
            BindingSource.Form,
            form,
            CultureInfo.InvariantCulture
        );
        await services
            .GetRequiredService<IModelBinderFactory>()
            .CreateBinder(new ModelBinderFactoryContext { Metadata = metadata })
            .BindModelAsync(binding);

        binding
            .Result.Model.ShouldBeOfType<Dictionary<string, string>>()
            .ShouldBe(request.PropertyList);
    }

    [Fact]
    public async Task BodyOnly_KeepsJsonRouteAndDottedPropertyKeys()
    {
        var handler = new CaptureHandler();
        await using var services = Services(handler);
        await services
            .GetRequiredService<IAltinnMeldingerAdapter>()
            .CreateCorrespondence(Request([]));
        handler.Path.ShouldBe("/correspondence/api/v1/correspondence");
        handler.MediaType.ShouldStartWith("application/json");
        using var json = JsonDocument.Parse(handler.Bytes);
        json.RootElement.GetProperty("correspondence")
            .GetProperty("propertyList")
            .GetProperty(FingerprintKey)
            .GetString()
            .ShouldBe(new string('A', 64));
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(409)]
    [InlineData(422)]
    [InlineData(500)]
    public async Task Upload_PreservesUpstreamProblemDetails(int status)
    {
        var handler = new CaptureHandler { Status = (HttpStatusCode)status };
        await using var services = Services(handler);
        List<IFormFile> files = [File("synthetic.pdf")];
        var exception = await Should.ThrowAsync<ApiException>(() =>
            services
                .GetRequiredService<IAltinnMeldingerAdapter>()
                .CreateCorrespondence(Request(files), files)
        );
        exception.ResponseStatusCode.ShouldBe(status);
        var problem = exception.GetAltinnProblemDetails().ShouldNotBeNull();
        problem.ErrorCode.ShouldBe("1005");
        problem.TraceId.ShouldBe("synthetic-trace");
        problem.Detail.ShouldBe("Synthetic failure");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Upload_ReplaysNonSeekableFilesAndDisposesOpenedStreams(bool failure)
    {
        var stream = new NonSeekableFile(Pdf(0));
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("synthetic.pdf");
        file.ContentType.Returns("application/pdf");
        file.Length.Returns(Pdf(0).Length);
        file.OpenReadStream().Returns(stream);
        List<IFormFile> files = [file];
        var handler = new CaptureHandler
        {
            FailFirst = true,
            Status = failure ? HttpStatusCode.InternalServerError : HttpStatusCode.OK,
        };
        await using var services = Services(handler);
        var adapter = services.GetRequiredService<IAltinnMeldingerAdapter>();
        if (failure)
            await Should.ThrowAsync<ApiException>(() =>
                adapter.CreateCorrespondence(Request(files), files)
            );
        else
            await adapter.CreateCorrespondence(Request(files), files);

        handler.Attempts.Count.ShouldBe(2);
        handler.Attempts[1].ShouldBe(handler.Attempts[0]);
        var form = await Context(handler, services).Request.ReadFormAsync(Token);
        form.Files.ShouldHaveSingleItem().Length.ShouldBe(Pdf(0).Length);
        stream.Disposed.ShouldBeTrue();
    }

    [Fact]
    public async Task Upload_HonorsCancellationBeforeSending()
    {
        var handler = new CaptureHandler();
        await using var services = Services(handler);
        List<IFormFile> files = [File("synthetic.pdf")];
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            services
                .GetRequiredService<IAltinnCorrespondenceClient>()
                .InitializeCorrespondence(Request(files).ToApiRequest(), files, cancellation.Token)
        );
        handler.Attempts.ShouldBeEmpty();
    }

    private static CorrespondenceRequest Request(List<IFormFile> files) =>
        new()
        {
            SendersReference = "synthetic-reference",
            IdempotentKey = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            Recipients = [new Organization { OrgNumber = "123456789" }],
            PropertyList = new() { [FingerprintKey] = new string('A', 64), ["caseId"] = "case-1" },
            Content = new InitializeCorrespondenceContent
            {
                MessageTitle = "Synthetic",
                MessageBody = "Synthetic",
                Language = "nb",
                Attachments = files
                    .Select(
                        (file, i) =>
                            new InitializeCorrespondenceAttachment
                            {
                                DataLocationType =
                                    InitializeAttachmentDataLocationType.NewCorrespondenceAttachment,
                                FileName = file.FileName,
                                DisplayName = $"Display {i}",
                                SendersReference = "synthetic-reference",
                            }
                    )
                    .ToList(),
            },
        };

    private static ServiceProvider Services(CaptureHandler handler)
    {
        var token = Substitute.For<IAltinnTokenProvider>();
        token
            .GetToken(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
                string.Join(
                    ' ',
                    call.Arg<IReadOnlyList<string>>()
                        ?? throw new InvalidOperationException("Missing scopes")
                )
            );
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns("Staging");
        var services = new ServiceCollection().AddLogging();
        services.AddMvcCore();
        services.AddSingleton(token);
        services
            .AddAltinn(
                environment,
                new MaskinportenConfiguration
                {
                    IntegrationId = "synthetic",
                    PrivateKey = "synthetic",
                    Scopes = ["altinn:serviceowner", "altinn:correspondence.write"],
                }
            )
            .AddMeldingerAdapter();
        services
            .AddHttpClient("AltinnCorrespondenceApiClient")
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        services.Configure<HttpStandardResilienceOptions>(
            "AltinnCorrespondenceApiClient-standard",
            options =>
            {
                options.Retry.MaxRetryAttempts = 1;
                options.Retry.Delay = TimeSpan.Zero;
                options.Retry.UseJitter = false;
                options.Retry.ShouldHandle = args =>
                    ValueTask.FromResult(
                        handler.FailFirst
                            && args.Outcome.Result?.StatusCode == HttpStatusCode.InternalServerError
                    );
            }
        );
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext Context(CaptureHandler handler, IServiceProvider services)
    {
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.ContentType = handler.MediaType;
        context.Request.Body = new MemoryStream(handler.Bytes);
        return context;
    }

    private static ModelBindingContext Binding(
        HttpContext context,
        ModelMetadata metadata,
        string name
    ) =>
        DefaultModelBindingContext.CreateBindingContext(
            new ActionContext { HttpContext = context },
            new CompositeValueProvider(),
            metadata,
            new BindingInfo(),
            name
        );

    private static IFormFile File(string name, int index = 0)
    {
        var pdf = Pdf(index);
        return new FormFile(new MemoryStream(pdf), 0, pdf.Length, $"file-{index}", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf",
        };
    }

    private static byte[] Pdf(int index)
    {
        var document = new StringBuilder($"%PDF-1.4\n% Synthetic {index}\n");
        string[] objects =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 72 72] /Contents 4 0 R >>",
            "<< /Length 0 >>\nstream\nendstream",
        ];
        var offsets = new List<int>();
        for (var i = 0; i < objects.Length; i++)
        {
            offsets.Add(document.Length);
            document.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }
        var xref = document.Length;
        document.Append("xref\n0 5\n0000000000 65535 f \n");
        foreach (var offset in offsets)
            document.Append($"{offset:D10} 00000 n \n");
        document.Append($"trailer\n<< /Size 5 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return Encoding.ASCII.GetBytes(document.ToString());
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public HttpStatusCode Status { get; init; } = HttpStatusCode.OK;
        public bool FailFirst { get; init; }
        public List<byte[]> Attempts { get; } = [];
        public string? Path { get; private set; }
        public string? MediaType { get; private set; }
        public byte[] Bytes { get; private set; } = [];
        public string? Scopes { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Path = request.RequestUri!.AbsolutePath;
            MediaType = request.Content!.Headers.ContentType!.ToString();
            Bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            Attempts.Add(Bytes);
            Scopes = request.Headers.Authorization?.Parameter;
            var status =
                FailFirst && Attempts.Count == 1 ? HttpStatusCode.InternalServerError : Status;
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(
                    status == HttpStatusCode.OK
                        ? """{"correspondences":[{"correspondenceId":"11111111-2222-3333-4444-555555555555"}]}"""
                        : $$"""{"status":{{(int)status}},"errorCode":1005,"title":"Synthetic error","detail":"Synthetic failure","traceId":"synthetic-trace"}""",
                    Encoding.UTF8,
                    "application/json"
                ),
            };
        }
    }

    private sealed class NonSeekableFile(byte[] bytes) : MemoryStream(bytes)
    {
        public bool Disposed { get; private set; }
        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin loc) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
