using Bl.Webhook.Sample.Model;
using Bl.Webhook.Sample.Repository;

namespace Bl.Webhook.Sample.Services;

internal class WebhookService
{
    private readonly WebhookContext _context;

    public WebhookService(WebhookContext context)
    {
        _context = context;
    }

    public async Task AddSubscription(WebhookSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _context.WebhookSubscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<WebhookSubscription[]> GetSubscriptionsAsync(string eventType, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _context.WebhookSubscriptions
            .Where(s => s.EventType == eventType)
            .ToArrayAsync(cancellationToken);

        return subscriptions;
    }
}
