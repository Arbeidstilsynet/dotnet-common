using Microsoft.Extensions.Hosting;

namespace Arbeidstilsynet.Common.MeldingerReceiver.Implementation;

internal class ReceiverListener(
    IMeldingerReceiver meldingerReceiver,
    IMeldingerConsumer meldingerConsumer
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!string.IsNullOrEmpty(meldingerConsumer.AppId))
            {
                await meldingerConsumer.ConsumeNewNotifications(
                    await meldingerReceiver.GetNotifications(meldingerConsumer.AppId)
                );
            }
            Thread.Sleep(
                meldingerConsumer.PollInterval == null || meldingerConsumer.PollInterval < 1000
                    ? 1000
                    : (int)meldingerConsumer.PollInterval
            );
        }
    }
}
