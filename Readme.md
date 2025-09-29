# Bl.Webhook.Sample

Webhook usage example for simple scenarios.

## Getting Started

Before running the sample, make sure you have Docker and Docker Compose installed on your machine.

Run the docker compose with the follow command:
```bash
docker-compose up -d
```

## Webhook structure

The project is structured as follows:
- `WebhookContext` class: This class is responsible for managing the postgre storage.
- `WebhookService` class: This class is responsible for managing the webhook operations.
	- `AddSubscription` method: This method adds a new webhook subscription.
	- `GetSubscriptionsAsync` method: This method retrieves all webhook subscriptions by the event name.
- `WebhookDispatcherService` class: This class is responsible for dispatching the webhook events.
	- `DispatchAsync` method: This method dispatches the webhook event to all subscribers, this method sends the event to a `System.Threading.Channels.Channel`.
	- `ProcessAsync` method: This method processes an unit of event for all the subscriptions, this method is called by a background service.
- `WebhookDispatcherWorker` class: This class is a background service that continuously processes webhook events from the channel.