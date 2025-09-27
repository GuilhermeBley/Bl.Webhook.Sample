using Bl.Webhook.Sample.Model;
using Bl.Webhook.Sample.Repository;
using System.Threading.Channels;

namespace Bl.Webhook.Sample.Services;

internal record WebhookDispatch(string EventType, object? Data);

internal class WebhookDispatcherService
{
    public const string HttpClientName = "webhook_dispatcher";

    private readonly WebhookContext _context;
    private readonly Channel<WebhookDispatch> _channel;
    private readonly HttpClient _httpClient;

    public WebhookDispatcherService(
        Channel<WebhookDispatch> channel,
        IHttpClientFactory httpClientFactory,
        WebhookContext context)
    {
        _channel = channel;
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _context = context;
    }

    public Task DispatchAsync<T>(
        string eventType,
        T payload,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        return _channel.Writer.WriteAsync(
            new WebhookDispatch(eventType, payload),
            cancellationToken)
            .AsTask();
    }

    public async Task ProcessAsync(
        string eventType,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await GetAllSubscriptions(eventType, cancellationToken);
        foreach (var subscription in subscriptions)
        {
            var request = new
            {
                Id = Guid.NewGuid(),
                WebhookId = subscription.Id,
                EventType = eventType,
                SubscriptionId = subscription.Id,
                Payload = payload,
                CreatedAt = DateTime.UtcNow
            };

            await _httpClient.PostAsJsonAsync(
                subscription.WebhookUrl,
                request,
                cancellationToken);
        }
    }

    private async Task<WebhookSubscription[]> GetAllSubscriptions(
        string eventType,
        CancellationToken cancellationToken = default)
    {
        List<WebhookSubscription> subscriptions = new();
        try
        {
            var subs = await _context.WebhookSubscriptions
                .AsNoTracking()
                .Where(s => s.EventType == eventType)
                .ToArrayAsync(cancellationToken);

            subscriptions.AddRange(subs);

        } catch { }

        await Task.CompletedTask;

        return subscriptions.ToArray();
    }
}
