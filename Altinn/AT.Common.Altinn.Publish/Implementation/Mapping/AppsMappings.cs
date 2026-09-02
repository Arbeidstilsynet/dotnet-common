using Arbeidstilsynet.Common.Altinn.Model.Api.Response;
using GeneratedAppsInstance = Arbeidstilsynet.Common.Altinn.Apps.Models.Instance;

namespace Arbeidstilsynet.Common.Altinn.Implementation.Mapping;

/// <summary>
/// Maps the generated apps models onto the package's own models.
/// </summary>
/// <remarks>
/// The apps API declares its own instance schema, separate from the storage API's, so it generates
/// a distinct type despite describing the same resource.
/// </remarks>
internal static class AppsMappings
{
    public static AltinnInstance ToAltinnInstance(this GeneratedAppsInstance source)
    {
        return new AltinnInstance
        {
            Id = source.Id,
            AppId = source.AppId,
            Org = source.Org,
            DueBefore = source.DueBefore?.DateTime,
            VisibleAfter = source.VisibleAfter?.DateTime,
            InstanceOwner = source.InstanceOwner is { } owner
                ? new InstanceOwner
                {
                    PartyId = owner.PartyId,
                    PersonNumber = owner.PersonNumber,
                    OrganisationNumber = owner.OrganisationNumber,
                    Username = owner.Username,
                }
                : null!,
            Process = source.Process is { } process
                ? new ProcessState
                {
                    Started = process.Started?.DateTime,
                    StartEvent = process.StartEvent,
                    Ended = process.Ended?.DateTime,
                    EndEvent = process.EndEvent,
                }
                : null!,
            Data = [],
            DataValues = [],
            PresentationTexts = [],
        };
    }
}
