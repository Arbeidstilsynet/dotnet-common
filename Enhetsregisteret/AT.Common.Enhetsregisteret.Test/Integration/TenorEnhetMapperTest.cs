using System.Threading.Tasks;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor;
using Arbeidstilsynet.Common.Enhetsregisteret.Test.Integration.Fixture;
using Bogus;
using Mapster;
using MapsterMapper;
using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Test.Unit;

public class TenorEnhetMapperTests : TestBed<EnhetsregisterTenorTestFixture>
{
    private IMapper _mapper;

    private readonly VerifySettings _verifySettings = new();

    private static readonly Faker<TenorEnhet> Faker = new Faker<TenorEnhet>()
        .UseSeed(1337)
        .RuleForType(typeof(string), x => "example-value")
        .RuleForType(typeof(int), x => 42)
        .RuleForType(
            typeof(Model.Tenor.Postadresse),
            x => new Model.Tenor.Postadresse
            {
                Land = "Norge",
                Landkode = "NO",
                Postnummer = "6470",
                Poststed = "ERESFJORD",
                Adresse = ["Vestre Feltet"],
                Kommune = "MOLDE",
                Kommunenummer = "1506",
            }
        )
        .RuleForType(
            typeof(Model.Tenor.Forretningsadresse),
            x => new Model.Tenor.Forretningsadresse
            {
                Land = "Norge",
                Landkode = "NO",
                Postnummer = "6470",
                Poststed = "ERESFJORD",
                Adresse = ["Vestre Feltet"],
                Kommune = "MOLDE",
                Kommunenummer = "1506",
            }
        )
        .RuleFor(
            x => x.Naeringskoder,
            faker =>
                [
                    new Naeringskoder
                    {
                        Kode = "79",
                        Beskrivelse = "Beskrivelse 1",
                        Nivaa = 1,
                    },
                    new Naeringskoder
                    {
                        Kode = "79.9",
                        Beskrivelse = "Beskrivelse 2",
                        Nivaa = 2,
                    },
                ]
        )
        .RuleFor(x => x.InstitusjonellSektorkode, faker => new InstitusjonellSektorkode { })
        .RuleFor(x => x.Organisasjonsform, faker => new Model.Tenor.Organisasjonsform { });

    public TenorEnhetMapperTests(
        ITestOutputHelper testOutputHelper,
        EnhetsregisterTenorTestFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _mapper = fixture.GetService<IMapper>(testOutputHelper)!;
        _verifySettings.UseDirectory("TestData/Snapshots");
    }

    [Fact]
    public async Task MapTenorEnhet_To_BrregEnhet()
    {
        //arrange
        var tenorEnhet = Faker.Generate(1)[0];
        tenorEnhet.Underenhet = null;
        //act
        var result = _mapper.Map<Enhet>(tenorEnhet);
        //assert
        await Verify(result, _verifySettings);
    }

    [Fact]
    public async Task MapTenorEnhet_To_BrregUnderenhet()
    {
        //arrange
        var tenorEnhet = Faker.Generate(1)[0];
        tenorEnhet.Underenhet = new Model.Tenor.Underenhet { Hovedenhet = "123123123" };
        //act
        var result = _mapper.Map<Model.Brreg.Underenhet>(tenorEnhet);
        //assert
        await Verify(result, _verifySettings);
    }
}
