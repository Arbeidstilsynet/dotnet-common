using Arbeidstilsynet.Common.Saksarkiv.V3;
using Arbeidstilsynet.Common.Saksarkiv.V3.Models.AT.EArkiv.Entiteter.API.V3;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;
using Shouldly;

namespace Arbeidstilsynet.Common.Saksarkiv.Test.Unit;

public class OpprettSakRequestSerializationTests
{
    private static string SerializeMultipart(
        OpprettSakRequest request,
        IEnumerable<SaksarkivFile>? files = null
    )
    {
        var adapter = new DefaultRequestAdapter(new AnonymousAuthenticationProvider());
        var body = SaksarkivClientV3Extensions.BuildOpprettSakBody(request, files);

        var requestInfo = new RequestInformation(
            Method.POST,
            "https://saksarkiv.example.com/api/v3/saker",
            new Dictionary<string, object>()
        );
        requestInfo.SetContentFromParsable(adapter, "multipart/form-data", body);

        using var reader = new StreamReader(requestInfo.Content);
        return reader.ReadToEnd();
    }

    [Fact]
    public void BuildOpprettSakBody_SerializesPayloadAsJsonPart()
    {
        var request = new OpprettSakRequest
        {
            Tittel = "Min sak",
            Saksbehandler = "user@arbeidstilsynet.no",
            AnsvarligEnhetKode = "ABC",
            EksternId = "ext-1",
            Sakstype = "type",
            Arkivkode = "code",
            Tilgangskode = "U",
            Status = Sakstatus.Avsluttet,
        };

        var content = SerializeMultipart(request);

        content.ShouldContain("name=\"payload\"");
        content.ShouldContain("Content-Type: application/json");
        content.ShouldContain("\"tittel\":\"Min sak\"");
        content.ShouldContain("\"ansvarligEnhetKode\":\"ABC\"");
        content.ShouldContain("\"eksternId\":\"ext-1\"");
        content.ShouldContain("\"status\":\"Avsluttet\"");
    }

    [Fact]
    public void BuildOpprettSakBody_IncludesUploadedFilesAsParts()
    {
        var request = new OpprettSakRequest
        {
            Tittel = "t",
            Saksbehandler = "s",
            AnsvarligEnhetKode = "e",
            EksternId = "ext-1",
            Sakstype = "type",
            Arkivkode = "code",
            Tilgangskode = "U",
        };

        var files = new[]
        {
            new SaksarkivFile
            {
                FileName = "hoved.pdf",
                Content = new MemoryStream("hello"u8.ToArray()),
                ContentType = "application/pdf",
            },
            new SaksarkivFile
            {
                FileName = "vedlegg.txt",
                Content = new MemoryStream("world"u8.ToArray()),
                ContentType = "text/plain",
            },
        };

        var content = SerializeMultipart(request, files);

        content.ShouldContain("name=\"filer\"; filename=\"hoved.pdf\"");
        content.ShouldContain("name=\"filer\"; filename=\"vedlegg.txt\"");
        content.ShouldContain("Content-Type: application/pdf");
        content.ShouldContain("Content-Type: text/plain");
        content.ShouldContain("hello");
        content.ShouldContain("world");
    }
}
