using System.ComponentModel.DataAnnotations;

namespace Bl.Webhook.Sample.Model;

public class WebhookSubscription
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateWebhookSubscriptionViewModel
{
    [Required]
    public string EventType { get; set; } = string.Empty;
    [Required, Url]
    public string WebhookUrl { get; set; } = string.Empty;
}
