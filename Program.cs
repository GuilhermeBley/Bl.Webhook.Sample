using Bl.Webhook.Sample.Model;
using Bl.Webhook.Sample.Repository;
using Bl.Webhook.Sample.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Bl.Webhook.Sample.Repository.WebhookContext>(cfg =>
{
    cfg.UseNpgsql(builder.Configuration.GetConnectionString("DbContext")
        ?? throw new InvalidOperationException("Connection string 'DbContext' not found."))
#if DEBUG
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
#endif
    ;
});

builder.Services.AddSingleton<InMemoryWebhookService>();
builder.Services.AddSingleton(_ => 
{
    return Channel.CreateBounded<WebhookDispatch>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait,
    });
});

builder.Services.AddScoped<WebhookDispatcherService>();
builder.Services.AddHttpClient(
    WebhookDispatcherService.HttpClientName,
    cfg =>
    {
        cfg.Timeout = TimeSpan.FromSeconds(10);
        cfg.DefaultRequestHeaders.UserAgent.ParseAdd("Bl.Webhook");
        cfg.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            UseCookies = false,
            AllowAutoRedirect = false,  
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("product/radom", async (
    [FromServices] WebhookDispatcherService dispatcher,
    [FromServices] WebhookContext context,
    CancellationToken cancellationToken) =>
{
    var inserResult =
        await context
        .Products
        .AddAsync(new ProductModel
        {
            Name = "Random Product",
        }, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    await dispatcher.DispatchAsync(
        "product.create",
        inserResult.Entity,
        cancellationToken: cancellationToken);

    return Results.Created();
})
.WithName("Create random product")
.WithOpenApi();

app.MapPost("webhook/in-memory", (
    [FromBody] CreateWebhookSubscriptionViewModel model,
    [FromServices] InMemoryWebhookService webhookService) =>
{
    webhookService.AddSubscription(new WebhookSubscription
    {
        Id = Guid.NewGuid(),
        EventType = model.EventType,
        WebhookUrl = model.WebhookUrl,
        CreatedAt = DateTime.UtcNow
    });

    return Results.Created();
})
.WithName("Subscribe to inMemory WebHook")
.WithOpenApi();

app.MapPost("webhook", async (
    [FromBody] CreateWebhookSubscriptionViewModel model,
    [FromServices] WebhookService webhookService,
    CancellationToken cancellationToken = default) =>
{
    await webhookService.AddSubscription(new WebhookSubscription
    {
        Id = Guid.NewGuid(),
        EventType = model.EventType,
        WebhookUrl = model.WebhookUrl,
        CreatedAt = DateTime.UtcNow
    }, cancellationToken);

    return Results.Created();
})
.WithName("Subscribe to WebHook")
.WithOpenApi();

app.Run();
