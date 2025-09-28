using Bl.Webhook.Sample.Model;

namespace Bl.Webhook.Sample.Repository;

public class WebhookContext : DbContext
{
    public DbSet<ProductModel> Products { get; set; }
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }

    public WebhookContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WebhookSubscription>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();

            b.Property(e => e.EventType).HasMaxLength(255).IsRequired();
            b.Property(e => e.WebhookUrl).HasMaxLength(2048).IsRequired();
            b.Property(e => e.CreatedAt).IsRequired();

            b.HasIndex(e => e.EventType);
        });

        modelBuilder.Entity<ProductModel>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();

            b.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });
    }
}
