using System.Net;
using Arbeidstilsynet.Common.Altinn.Correspondence.Models;
using Arbeidstilsynet.Common.Altinn.DependencyInjection;
using Arbeidstilsynet.Common.Altinn.Events.Models;
using Arbeidstilsynet.Common.Altinn.Implementation.Adapter;
using Arbeidstilsynet.Common.Altinn.Implementation.Configuration;
using Arbeidstilsynet.Common.Altinn.Model.Adapter;
using Arbeidstilsynet.Common.Altinn.Ports.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace Arbeidstilsynet.Common.Altinn.Test.Unit;

/// <summary>
/// Covers the adapters' use of a missing resource as control flow.
/// </summary>
/// <remarks>
/// Several Altinn specifications describe their error responses in a way Kiota cannot turn into a
/// typed error, reporting "Could not create error type" during generation, so these arrive as a
/// bare <see cref="ApiException"/> carrying only a status code.
/// </remarks>
public class ApiExceptionMappingTests
{
    private readonly IAltinnCorrespondenceClient _correspondenceClient =
        Substitute.For<IAltinnCorrespondenceClient>();
    private readonly IAltinnEventsClient _eventsClient = Substitute.For<IAltinnEventsClient>();
    private readonly IAltinnStorageClient _storageClient = Substitute.For<IAltinnStorageClient>();

    private readonly AltinnMeldingerAdapter _meldingerAdapter;
    private readonly AltinnAdapter _altinnAdapter;

    public ApiExceptionMappingTests()
    {
        _meldingerAdapter = new AltinnMeldingerAdapter(_correspondenceClient);

        _altinnAdapter = new AltinnAdapter(
            _storageClient,
            _eventsClient,
            Options.Create(new AltinnConfiguration()),
            new ResolvedAltinnUrls
            {
                AuthenticationUrl = new Uri("https://platform.altinn.no/authentication/api/v1"),
                StorageUrl = new Uri("https://platform.altinn.no/storage/api/v1"),
                EventsUrl = new Uri("https://platform.altinn.no/events/api/v1"),
                CorrespondenceUrl = new Uri("https://platform.altinn.no"),
                DialogportenUrl = new Uri("https://platform.altinn.no/dialogporten"),
                AppBaseUrl = new Uri("https://dat.apps.altinn.no/"),
                MaskinportenUrl = new Uri("https://maskinporten.no/"),
            },
            Substitute.For<ILogger<AltinnAdapter>>()
        );
    }

    private static ApiException NotFound() =>
        new() { ResponseStatusCode = (int)HttpStatusCode.NotFound };

    private static ApiException ServerError() =>
        new() { ResponseStatusCode = (int)HttpStatusCode.InternalServerError };

    [Fact]
    public async Task GetCorrespondence_ReturnsNull_WhenNotFound()
    {
        _correspondenceClient
            .GetCorrespondence(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(NotFound());

        var result = await _meldingerAdapter.GetCorrespondence(Guid.NewGuid());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCorrespondence_Rethrows_WhenTheRequestFailsForAnotherReason()
    {
        _correspondenceClient
            .GetCorrespondence(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(ServerError());

        // A server error must not be reported as "no such correspondence".
        var exception = await Should.ThrowAsync<ApiException>(() =>
            _meldingerAdapter.GetCorrespondence(Guid.NewGuid())
        );

        exception.ResponseStatusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetCorrespondence_ReturnsOverview_WhenFound()
    {
        var overview = new CorrespondenceOverviewExt();

        _correspondenceClient
            .GetCorrespondence(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(overview);

        var result = await _meldingerAdapter.GetCorrespondence(Guid.NewGuid());

        result.ShouldBe(overview);
    }

    [Fact]
    public async Task GetAltinnSubscription_ReturnsNull_WhenNotFound()
    {
        _eventsClient
            .GetAltinnSubscription(123, Arg.Any<CancellationToken>())
            .ThrowsAsync(NotFound());

        var result = await _altinnAdapter.GetAltinnSubscription(123);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAltinnSubscription_Rethrows_WhenTheRequestFailsForAnotherReason()
    {
        _eventsClient
            .GetAltinnSubscription(123, Arg.Any<CancellationToken>())
            .ThrowsAsync(ServerError());

        await Should.ThrowAsync<ApiException>(() => _altinnAdapter.GetAltinnSubscription(123));
    }

    [Fact]
    public async Task UnsubscribeForCompletedProcessEvents_ReturnsFalse_WhenSubscriptionHasNoId()
    {
        var result = await _altinnAdapter.UnsubscribeForCompletedProcessEvents(new Subscription());

        result.ShouldBeFalse();
        await _eventsClient
            .DidNotReceive()
            .Unsubscribe(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
