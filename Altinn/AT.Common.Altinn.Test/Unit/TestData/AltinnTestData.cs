using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using Bogus;
using Bogus.Extensions.UnitedStates;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit.TestData;

internal static class AltinnTestData
{
    private static readonly Faker Faker = new();

    public static AltinnInstance CreateAltinnInstance(
        Guid? id = null,
        string? appId = null,
        string? org = null,
        string? partyId = null,
        string? organisationNumber = null,
        DateTime? processStarted = null,
        DateTime? processEnded = null,
        List<DataElement>? dataElements = null,
        Dictionary<string, string>? dataValues = null
    )
    {
        var instanceGuid = id ?? Faker.Random.Guid();
        var actualPartyId = partyId ?? Faker.Random.Number(10000, 99999).ToString();
        var actualOrg = org ?? "dat";
        var actualAppId = appId ?? $"{actualOrg}/{Faker.Lorem.Word()}-app";
        
        var processStart = processStarted ?? Faker.Date.Recent(30);
        var processEnd = processEnded ?? Faker.Date.Between(processStart, DateTime.UtcNow);
        
        return new AltinnInstance
        {
            Id = $"{actualPartyId}/{instanceGuid}",
            AppId = actualAppId,
            Org = actualOrg,
            InstanceOwner = new InstanceOwner
            {
                PartyId = actualPartyId,
                OrganisationNumber = organisationNumber ?? Faker.Company.Ein()
            },
            Process = new ProcessState
            {
                Started = processStart,
                Ended = processEnd
            },
            Data = dataElements ?? CreateDefaultDataElements(),
            DataValues = dataValues ?? new Dictionary<string, string>(),
            SelfLinks = new ResourceLinks
            {
                Apps = $"https://{actualOrg}.apps.altinn.no/{actualAppId}/instances/{actualPartyId}/{instanceGuid}",
                Platform = $"https://platform.altinn.no/storage/api/v1/instances/{actualPartyId}/{instanceGuid}"
            }
        };
    }

    public static List<DataElement> CreateDefaultDataElements(
        string? mainPdfDataTypeId = null,
        string? structuredDataTypeId = null,
        int attachmentCount = 0
    )
    {
        var dataElements = new List<DataElement>
        {
            CreateDataElement(mainPdfDataTypeId ?? "ref-data-as-pdf", "application/pdf")
        };

        if (structuredDataTypeId != null)
        {
            dataElements.Add(CreateDataElement(structuredDataTypeId, "application/json"));
        }

        for (int i = 0; i < attachmentCount; i++)
        {
            dataElements.Add(CreateDataElement($"attachment-{i}", Faker.PickRandom("application/pdf", "application/xml", "image/jpeg")));
        }

        return dataElements;
    }

    public static DataElement CreateDataElement(
        string dataType,
        string contentType = "application/json",
        string? filename = null,
        FileScanResult fileScanResult = FileScanResult.Clean
    )
    {
        return new DataElement
        {
            Id = Faker.Random.Guid().ToString(),
            DataType = dataType,
            ContentType = contentType,
            Filename = filename ?? $"{dataType}.{GetExtensionFromContentType(contentType)}",
            FileScanResult = fileScanResult,
            Size = Faker.Random.Long(1024, 10485760), // 1KB to 10MB
            Locked = Faker.Random.Bool(0.1f), // 10% chance of being locked
            IsRead = Faker.Random.Bool(0.8f) // 80% chance of being read
        };
    }

    public static AltinnCloudEvent CreateAltinnCloudEvent(
        string? subject = null,
        string? type = null,
        string? appId = null,
        string? partyId = null
    )
    {
        var actualPartyId = partyId ?? Faker.Random.Number(10000, 99999).ToString();
        var actualAppId = appId ?? $"dat/{Faker.Lorem.Word()}-app";
        
        return new AltinnCloudEvent
        {
            Id = Faker.Random.Guid().ToString(),
            Source = new Uri($"https://dat.apps.altinn.no/{actualAppId}"),
            SpecVersion = "1.0",
            Type = type ?? "app.instance.process.completed",
            Subject = subject ?? $"/party/{actualPartyId}",
            Time = Faker.Date.Recent(7),
        };
    }

    public static AltinnSubscription CreateAltinnSubscription(
        int id = 0,
        Uri? endPoint = null,
        Uri? sourceFilter = null,
        string? typeFilter = null,
        string? consumer = null
    )
    {
        var subscriptionId = id > 0 ? id : Faker.Random.Number(1, 10000);
        
        return new AltinnSubscription
        {
            Id = subscriptionId,
            EndPoint = endPoint ?? new Uri($"https://{Faker.Internet.DomainName()}/callback"),
            SourceFilter = sourceFilter ?? new Uri($"https://dat.apps.altinn.no/dat/{Faker.Lorem.Word()}-app"),
            TypeFilter = typeFilter ?? "app.instance.process.completed",
            Consumer = consumer ?? $"/org/{Faker.Company.CompanyName().ToLower()}"
        };
    }

    public static SubscriptionRequestDto CreateSubscriptionRequestDto(
        Uri? callbackUrl = null,
        string? altinnAppId = null
    )
    {
        return new SubscriptionRequestDto
        {
            CallbackUrl = callbackUrl ?? new Uri($"https://{Faker.Internet.DomainName()}/webhook/altinn"),
            AltinnAppId = altinnAppId ?? $"{Faker.Lorem.Word()}-app"
        };
    }

    public static InstanceQueryParameters CreateInstanceQueryParameters(
        string? appId = null,
        string? org = null,
        bool processIsComplete = true,
        string? excludeConfirmedBy = null
    )
    {
        var actualOrg = org ?? "dat";
        
        return new InstanceQueryParameters
        {
            AppId = appId ?? $"{actualOrg}/{Faker.Lorem.Word()}-app",
            Org = actualOrg,
            ProcessIsComplete = processIsComplete,
            ExcludeConfirmedBy = excludeConfirmedBy ?? actualOrg
        };
    }

    public static Faker<AltinnInstance> GetAltinnInstanceFaker(
        string? org = null,
        string? appId = null
    )
    {
        var actualOrg = org ?? "dat";
        var actualAppId = appId ?? $"{actualOrg}/{{appName}}-app";
        
        return new Faker<AltinnInstance>()
            .RuleFor(x => x.Id, f => $"{f.Random.Number(10000, 99999)}/{f.Random.Guid()}")
            .RuleFor(x => x.AppId, f => actualAppId.Replace("{appName}", f.Lorem.Word()))
            .RuleFor(x => x.Org, actualOrg)
            .RuleFor(x => x.InstanceOwner, f => new InstanceOwner
            {
                PartyId = f.Random.Number(10000, 99999).ToString(),
                OrganisationNumber = f.Company.Ein()
            })
            .RuleFor(x => x.Process, f =>
            {
                var start = f.Date.Recent(30);
                return new ProcessState
                {
                    Started = start,
                    Ended = f.Date.Between(start, DateTime.UtcNow)
                };
            })
            .RuleFor(x => x.Data, f => CreateDefaultDataElements())
            .RuleFor(x => x.DataValues, f => new Dictionary<string, string>())
            .RuleFor(x => x.SelfLinks, (f, instance) => new ResourceLinks
            {
                Apps = $"https://{actualOrg}.apps.altinn.no/{instance.AppId}/instances/{instance.Id}",
                Platform = $"https://platform.altinn.no/storage/api/v1/instances/{instance.Id}"
            });
    }

    private static string GetExtensionFromContentType(string contentType)
    {
        return contentType switch
        {
            "application/pdf" => "pdf",
            "application/json" => "json",
            "application/xml" => "xml",
            "image/jpeg" => "jpg",
            "image/png" => "png",
            _ => "bin"
        };
    }
}