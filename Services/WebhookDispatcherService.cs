using Bl.Webhook.Sample.Model;

namespace Bl.Webhook.Sample.Services;

internal class WebhookDispatcherService
{
    public const string HttpClientName = "webhook_dispatcher";

    private readonly InMemoryWebhookService _inMemoryWebhook;
    private readonly HttpClient _httpClient;

    public WebhookDispatcherService(
        InMemoryWebhookService inMemoryWebhook,
        IHttpClientFactory httpClientFactory)
    {
        _inMemoryWebhook = inMemoryWebhook;
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
    }

    public async Task DispatchAsync(
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
            subscriptions.AddRange(_inMemoryWebhook.GetSubscriptions(eventType));

        } catch { }

        await Task.CompletedTask;

        return subscriptions.ToArray();
    }
}
