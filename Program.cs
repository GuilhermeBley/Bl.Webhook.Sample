using Bl.Webhook.Sample.Model;
using Bl.Webhook.Sample.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<InMemoryWebhookService>();
builder.Services.AddSingleton<WebhookDispatcherService>();
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

app.MapPost("/product/radom", async (
    [FromServices] WebhookDispatcherService dispatcher,
    CancellationToken cancellationToken) =>
{
    await dispatcher.DispatchAsync(
        "product.create",
        new { Id = Guid.NewGuid(), Name = "Random Product", CreatedAt = DateTime.UtcNow },
        cancellationToken: cancellationToken);

    return Results.Created();
})
.WithName("Subscribe to inMemory WebHook")
.WithOpenApi();

app.MapPost("/webhook/in-memory", (
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

app.Run();
