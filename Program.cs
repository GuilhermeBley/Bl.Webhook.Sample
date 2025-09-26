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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/webhook/in-memory", (
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
