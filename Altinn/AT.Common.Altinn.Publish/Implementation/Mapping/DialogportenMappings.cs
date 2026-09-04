using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using GeneratedLocalization = Arbeidstilsynet.Common.Altinn.Dialogporten.Models.V1CommonLocalizations_Localization;
using GeneratedLookup = Arbeidstilsynet.Common.Altinn.Dialogporten.Models.V1CommonIdentifierLookup_ServiceOwnerIdentifierLookup;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Mapping;

/// <summary>
/// Maps the generated Dialogporten models onto the package's own models.
/// </summary>
internal static class DialogportenMappings
{
    public static DialogportenLookupResponse ToLookupResponse(this GeneratedLookup source)
    {
        return new DialogportenLookupResponse
        {
            DialogId = Guid.TryParse(source.DialogId, out var dialogId) ? dialogId : Guid.Empty,
            InstanceRef = source.InstanceRef ?? string.Empty,
            Party = source.Party ?? string.Empty,
            ServiceResource = source.ServiceResource is { } resource
                ? new DialogportenServiceResource
                {
                    Id = resource.Id ?? string.Empty,
                    IsDelegable = resource.IsDelegable ?? false,
                    MinimumAuthenticationLevel = resource.MinimumAuthenticationLevel ?? 0,
                    Name = ToLocalizations(resource.Name),
                }
                : null,
            ServiceOwner = source.ServiceOwner is { } owner
                ? new DialogportenServiceOwner
                {
                    OrgNumber = owner.OrgNumber ?? string.Empty,
                    Code = owner.Code ?? string.Empty,
                    Name = ToLocalizations(owner.Name),
                }
                : null,
            Title = ToLocalizations(source.Title),
            NonSensitiveTitle = ToLocalizations(source.NonSensitiveTitle),
        };
    }

    private static List<DialogportenLocalization>? ToLocalizations(
        List<GeneratedLocalization>? source
    )
    {
        if (source is null)
        {
            return null;
        }

        return
        [
            .. source.Select(localization => new DialogportenLocalization
            {
                Value = localization.Value ?? string.Empty,
                LanguageCode = localization.LanguageCode ?? string.Empty,
            }),
        ];
    }
}
