using System.Text.Json;
using Arbeidstilsynet.Common.MeldingerReceiver.Adapters.Test.fixtures;
using Arbeidstilsynet.Common.MeldingerReceiver.Implementation;
using Arbeidstilsynet.Common.MeldingerReceiver.Model;
using Shouldly;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Test;

public class MeldingerReceiverTests : TestBed<MeldingerReceiverFixture>
{
    private IMeldingerReceiver _meldingerReceiver;
    private IDatabase _testDatabase;

    public MeldingerReceiverTests(
        ITestOutputHelper testOutputHelper,
        MeldingerReceiverFixture fixture
    )
        : base(testOutputHelper, fixture)
    {
        _meldingerReceiver = fixture.GetService<IMeldingerReceiver>(testOutputHelper)!;
        _testDatabase = fixture.GetService<IConnectionMultiplexer>(testOutputHelper)!.GetDatabase();
    }

    [Fact]
    public async Task GetNotifications_WhenCalledWithNotificationForApp_ReturnsNotificationForFirstCall()
    {
        //arrange

        var testAppName = $"test-app-{Guid.NewGuid().ToString("n")[..8]}";
        var testDto = new MeldingerReceiverNotificationDto()
        {
            AppId = testAppName,
            MeldingId = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
        };
        await _testDatabase.StreamAddAsync(
            IConstants.StreamName,
            new NameValueEntry[] { new(IConstants.MessageKey, JsonSerializer.Serialize(testDto)) }
        );
        //act
        var result = await _meldingerReceiver.GetNotifications(testAppName);
        //assert
        result.ShouldNotBeEmpty();
        result.Values.First().ShouldBeEquivalentTo(testDto);

        var resultAfterNotificationWasAlreadyRetrieved = await _meldingerReceiver.GetNotifications(
            testAppName
        );
        resultAfterNotificationWasAlreadyRetrieved.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetNotifications_WhenCalledWithNoNotificationsForApp_ReturnsEmptyAndIdWasAcknowledged()
    {
        //arrange
        var testAppName = $"test-app-{Guid.NewGuid().ToString("n")[..8]}";
        var testDto = new MeldingerReceiverNotificationDto()
        {
            AppId = "non-existing-app-id",
            MeldingId = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
        };
        await _testDatabase.StreamAddAsync(
            IConstants.StreamName,
            new NameValueEntry[] { new(IConstants.MessageKey, JsonSerializer.Serialize(testDto)) }
        );
        //act
        var result = await _meldingerReceiver.GetNotifications(testAppName);
        var pendingMessages = await _meldingerReceiver.GetPendingMessages(testAppName);
        //assert
        result.ShouldBeEmpty();
        pendingMessages.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetNotifications_WhenCalledWithNoNotificationsForAppAndAckAfterwards_MakesNotificationNotAccessibleAnymore()
    {
        //arrange
        var testAppName = $"test-app-{Guid.NewGuid().ToString("n")[..8]}";
        var testDto = new MeldingerReceiverNotificationDto()
        {
            AppId = testAppName,
            MeldingId = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
        };
        await _testDatabase.StreamAddAsync(
            IConstants.StreamName,
            new NameValueEntry[] { new(IConstants.MessageKey, JsonSerializer.Serialize(testDto)) }
        );
        //act
        var result = await _meldingerReceiver.GetNotifications(testAppName);
        result.Count.ShouldBe(1);
        var pendingMessages = await _meldingerReceiver.GetPendingMessages(testAppName);
        pendingMessages.Length.ShouldBe(1);
        await _meldingerReceiver.AcknowledgeMessage(testAppName, pendingMessages.First().Id);
        var resultAfterAck = await _meldingerReceiver.GetPendingMessages(testAppName);
        //assert
        resultAfterAck.ShouldBeEmpty();
    }
}
