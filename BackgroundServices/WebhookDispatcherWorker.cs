using Bl.Webhook.Sample.Services;
using System.Threading.Channels;

namespace Bl.Webhook.Sample.BackgroundServices;

internal sealed class WebhookDispatcherWorker(
    Channel<WebhookDispatch> Channel,
    IServiceScopeFactory Factory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var dispatch in Channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = Factory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<WebhookDispatcherService>();
                await service.ProcessAsync(dispatch.EventType, dispatch.Data!, stoppingToken);
            }
            catch
            {
                // Log error
            }
        }
    }
}
