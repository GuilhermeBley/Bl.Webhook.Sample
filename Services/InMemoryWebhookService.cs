using Bl.Webhook.Sample.Model;

namespace Bl.Webhook.Sample.Services;

internal class InMemoryWebhookService
{
    private readonly List<WebhookSubscription> _subscriptions = new();

    public void AddSubscription(WebhookSubscription subscription)
    {
        _subscriptions.Add(subscription);
    }

    public WebhookSubscription[] GetSubscriptions(string eventType)
    {
        return _subscriptions.Where(s => s.EventType == eventType).ToArray();
    }
}
