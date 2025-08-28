using Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg;
using Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor;
using Mapster;

namespace Arbeidstilsynet.Common.Enhetsregisteret.Implementation;

internal class EnhetsMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<TenorEnhet, Enhet>()
            .NameMatchingStrategy(NameMatchingStrategy.Flexible)
            // Property name differences that won't match automatically
            .Map(dest => dest.Telefon, src => src.Telefonnummer)
            .Map(dest => dest.Mobil, src => src.Mobilnummer)
            .Map(dest => dest.Epostadresse, src => src.EPost)
            .Map(dest => dest.Hjemmeside, src => src.Nettadresse)
            .Map(
                dest => dest.OverordnetEnhet,
                src => src.Underenhet != null ? src.Underenhet.Hovedenhet : null
            )
            // Boolean conversions from string
            .Map(
                dest => dest.RegistrertIMvaregisteret,
                src => string.Equals("J", src.RegistrertIMvaregisteret)
            )
            .Map(
                dest => dest.RegistrertIStiftelsesregisteret,
                src => string.Equals("J", src.RegistrertIStiftelsesregisteret)
            )
            .Map(
                dest => dest.RegistrertIFrivillighetsregisteret,
                src => string.Equals("J", src.RegistrertIFrivillighetsregisteret)
            )
            .Map(
                dest => dest.RegistrertIForetaksregisteret,
                src => string.Equals("J", src.RegistrertIForetaksregisteret)
            )
            .Map(dest => dest.UnderAvvikling, src => string.Equals("J", src.UnderAvvikling))
            .Map(dest => dest.Konkurs, src => string.Equals("J", src.Konkurs))
            .Map(
                dest => dest.UnderTvangsavviklingEllerTvangsopplosning,
                src => string.Equals("J", src.UnderTvangsavviklingEllerTvangsopplosning)
            )
            // Complex object mappings
            .Map(
                dest => dest.Naeringskode1,
                src =>
                    src.Naeringskoder != null
                        ? src.Naeringskoder.FirstOrDefault(f => f.Nivaa == 1)
                        : null
            )
            .Map(
                dest => dest.Naeringskode2,
                src =>
                    src.Naeringskoder != null
                        ? src.Naeringskoder.FirstOrDefault(f => f.Nivaa == 2)
                        : null
            )
            .Map(
                dest => dest.Naeringskode3,
                src =>
                    src.Naeringskoder != null
                        ? src.Naeringskoder.FirstOrDefault(f => f.Nivaa == 3)
                        : null
            )
            // Array mappings
            .Map(
                dest => dest.VedtektsfestetFormaal,
                src =>
                    src.VedtektsfestetFormaal != null ? src.VedtektsfestetFormaal.ToArray() : null
            )
            .Map(
                dest => dest.Aktivitet,
                src => src.AktivitetBransje != null ? src.AktivitetBransje.ToArray() : null
            )
            // ignore fields whiche are not available on tenor set
            .Ignore(dest => dest.Hjelpeenhetskode)
            .Ignore(dest => dest.UnderAvviklingDato)
            .Ignore(dest => dest.Konkursdato)
            .Ignore(dest => dest.TvangsavvikletPgaManglendeSlettingDato)
            .Ignore(dest => dest.TvangsopplostPgaManglendeDagligLederDato)
            .Ignore(dest => dest.TvangsopplostPgaManglendeRevisorDato)
            .Ignore(dest => dest.TvangsopplostPgaManglendeRegnskapDato)
            .Ignore(dest => dest.TvangsopplostPgaMangelfulltStyreDato)
            .Ignore(dest => dest.FrivilligMvaRegistrertBeskrivelser)
            .Ignore(dest => dest.HarRegistrertAntallAnsatte)
            .Ignore(dest => dest.Slettedato)
            .Ignore(dest => dest.Links);

        config
            .NewConfig<TenorEnhet, Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg.Underenhet>()
            .NameMatchingStrategy(NameMatchingStrategy.Flexible)
            // Property name differences that won't match automatically
            .Map(dest => dest.Telefon, src => src.Telefonnummer)
            .Map(dest => dest.Mobil, src => src.Mobilnummer)
            .Map(dest => dest.Epostadresse, src => src.EPost)
            .Map(dest => dest.Hjemmeside, src => src.Nettadresse)
            .Map(dest => dest.Beliggenhetsadresse, src => src.Forretningsadresse)
            .Map(
                dest => dest.OverordnetEnhet,
                src => src.Underenhet != null ? src.Underenhet.Hovedenhet : null
            )
            // Boolean conversions from string
            .Map(
                dest => dest.RegistrertIMvaregisteret,
                src => string.Equals("J", src.RegistrertIMvaregisteret)
            )
            // Complex object mappings
            .Map(
                dest => dest.Naeringskode1,
                src =>
                    src.Naeringskoder != null
                        ? src.Naeringskoder.FirstOrDefault(f => f.Nivaa == 1)
                        : null
            )
            .Map(
                dest => dest.Naeringskode2,
                src =>
                    src.Naeringskoder != null
                        ? src.Naeringskoder.FirstOrDefault(f => f.Nivaa == 2)
                        : null
            )
            .Map(
                dest => dest.Naeringskode3,
                src =>
                    src.Naeringskoder != null
                        ? src.Naeringskoder.FirstOrDefault(f => f.Nivaa == 3)
                        : null
            )
            // ignore fields whiche are not available on tenor set
            .Ignore(dest => dest.FrivilligMvaRegistrertBeskrivelser)
            .Ignore(dest => dest.HarRegistrertAntallAnsatte)
            .Ignore(dest => dest.Oppstartsdato)
            .Ignore(dest => dest.DatoEierskifte)
            .Ignore(dest => dest.Nedleggelsesdato)
            .Ignore(dest => dest.Links)
            .Ignore(dest => dest.Hjelpeenhetskode);

        // Configure nested object mappings
        config
            .NewConfig<
                Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor.Organisasjonsform,
                Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg.Organisasjonsform
            >()
            .Ignore(dest => dest.Links)
            .Ignore(dest => dest.Utgaatt);
        config
            .NewConfig<
                Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor.Forretningsadresse,
                Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg.Adresse
            >()
            .Map(dest => dest.Gateadresse, src => src.Adresse);
        config
            .NewConfig<
                Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor.Postadresse,
                Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg.Adresse
            >()
            .Map(dest => dest.Gateadresse, src => src.Adresse);
        config.NewConfig<
            Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor.Naeringskoder,
            Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg.Naeringskode
        >();
        config.NewConfig<
            Arbeidstilsynet.Common.Enhetsregisteret.Model.Tenor.InstitusjonellSektorkode,
            Arbeidstilsynet.Common.Enhetsregisteret.Model.Brreg.Institusjonellsektorkode
        >();
    }
}
