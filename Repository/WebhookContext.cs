using Bl.Webhook.Sample.Model;

namespace Bl.Webhook.Sample.Repository;

public class WebhookContext : DbContext
{
    public DbSet<ProductModel> Products { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
}
