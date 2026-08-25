using System.Text.Json;
using Arbeidstilsynet.Common.Altinn.Model.Api.Request;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Pins that a real Altinn event binds to <see cref="AltinnCloudEvent"/> the way an
/// <c>[ApiController]</c> action would bind it.
/// </summary>
/// <remarks>
/// The generated <c>Events.Models.CloudEvent</c> looks like a replacement for this type but cannot
/// deserialize a real event, because the specification models <c>specversion</c> as an object
/// rather than the string Altinn sends. These tests exist so that swapping the two fails here
/// rather than in a consumer's webhook.
/// </remarks>
public class AltinnCloudEventBindingTests
{
    /// <summary>
    /// ASP.NET Core binds request bodies with <see cref="JsonSerializerDefaults.Web"/>, which
    /// differs from the serializer's own defaults in being case-insensitive.
    /// </summary>
    private static readonly JsonSerializerOptions BindingOptions = new(JsonSerializerDefaults.Web);

    private const string ProcessCompletedEvent = """
        {
          "id": "1ba7f8a4-0000-0000-0000-000000000001",
          "source": "https://dat.apps.altinn.no/dat/some-app/instances/51644866/11111111-2222-3333-4444-555555555555",
          "specversion": "1.0",
          "type": "app.instance.process.completed",
          "subject": "/party/51644866",
          "time": "2026-08-25T10:00:00Z"
        }
        """;

    private const string ValidateSubscriptionEvent = """
        {
          "id": "1ba7f8a4-0000-0000-0000-000000000002",
          "source": "https://platform.altinn.no/events/api/v1/subscriptions/123",
          "specversion": "1.0",
          "type": "platform.events.validatesubscription",
          "subject": "/party/51644866",
          "time": "2026-08-25T10:00:00Z"
        }
        """;

    [Fact]
    public void ProcessCompletedEvent_BindsEveryFieldTheAdapterReads()
    {
        var cloudEvent = JsonSerializer.Deserialize<AltinnCloudEvent>(
            ProcessCompletedEvent,
            BindingOptions
        );

        cloudEvent.ShouldNotBeNull();
        cloudEvent.Type.ShouldBe("app.instance.process.completed");
        cloudEvent.Id.ShouldBe("1ba7f8a4-0000-0000-0000-000000000001");
        cloudEvent.Subject.ShouldBe("/party/51644866");
        cloudEvent.SpecVersion.ShouldBe("1.0");

        // The instance addressing reads the source as a Uri, not a string.
        cloudEvent.Source.ShouldNotBeNull();
        cloudEvent.Source.PathAndQuery.ShouldContain(
            "/instances/51644866/11111111-2222-3333-4444-555555555555"
        );
    }

    [Fact]
    public void ValidateSubscriptionEvent_BindsItsType()
    {
        // Altinn requires a success response to this event before it activates a subscription, and
        // a webhook cannot recognise it without the type binding correctly.
        var cloudEvent = JsonSerializer.Deserialize<AltinnCloudEvent>(
            ValidateSubscriptionEvent,
            BindingOptions
        );

        cloudEvent.ShouldNotBeNull();
        cloudEvent.Type.ShouldBe("platform.events.validatesubscription");
    }

    [Fact]
    public void SpecVersion_IsAString_AsAltinnSends()
    {
        // The events specification declares specversion as an object. Altinn sends a string, so
        // this type must keep modelling it as one.
        var cloudEvent = JsonSerializer.Deserialize<AltinnCloudEvent>(
            ProcessCompletedEvent,
            BindingOptions
        );

        cloudEvent.ShouldNotBeNull();
        cloudEvent.SpecVersion.ShouldBeOfType<string>();
    }
}
