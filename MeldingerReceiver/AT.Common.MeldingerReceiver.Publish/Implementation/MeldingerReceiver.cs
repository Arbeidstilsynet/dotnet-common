using System.Text.Json;
using Arbeidstilsynet.Common.MeldingerReceiver.Model;
using StackExchange.Redis;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Implementation;

internal class MeldingerReceiver : IMeldingerReceiver
{
    private readonly IDatabase _db;

    private const string ConsumerName = "consumer_1";

    public MeldingerReceiver(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase();
    }

    public async Task<Dictionary<string, MeldingerReceiverNotificationDto>> GetNotifications(string groupName,
        string appId)
    {
        const string streamName = IConstants.StreamName;
        var resultMap = new Dictionary<string, MeldingerReceiverNotificationDto>();
        if (!(await _db.KeyExistsAsync(streamName)) ||
            (await _db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
        {
            await _db.StreamCreateConsumerGroupAsync(streamName, groupName, "0-0", true);
        }

        var results =
            (await _db.StreamReadGroupAsync(streamName, groupName, ConsumerName, ">", 10));
        var filteredResults = results.Where(r =>
                r.Values.Any(v => v.Name == IConstants.MessageKey)).ToArray();
        foreach (var entry in results.Except(filteredResults))
        {
            await AcknowledgeMessage(groupName, entry.Id!);
        }
        foreach (var result in filteredResults)
        {
            var value = result.Values.First(v => v.Name == IConstants.MessageKey).Value.ToString();
            var dto = JsonSerializer.Deserialize<MeldingerReceiverNotificationDto>(value);
            if (dto != null && dto.AppId == appId)
            {
                resultMap.Add(result.Id.ToString(), dto);
            }
            else
            {
                await AcknowledgeMessage(groupName, result.Id!);
            }
        }

        return resultMap;
    }

    public async Task<StreamEntry[]> GetPendingMessages(string groupName)
    {
        return await _db.StreamReadGroupAsync(IConstants.StreamName, groupName, ConsumerName, "0-0", count: 10);
    }
    

    public async Task<long> AcknowledgeMessage(string groupName, string messageId)
    {
        return await _db.StreamAcknowledgeAsync(IConstants.StreamName, groupName, messageId);
    }
}