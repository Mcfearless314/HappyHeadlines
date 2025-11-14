using EasyNetQ;
using EasyNetQ.Serialization.NewtonsoftJson;
using EasyNetQ.Topology;
using Newtonsoft.Json;
using Serilog.Sinks.File;

namespace PublisherService.Messaging;

public class MessageClient
{
    private readonly IBus _bus;

    public MessageClient(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T @event, string? queueName = null) where T : class
    {
        var properties = new MessageProperties();
        var message = new Message<T>(@event, properties);

        var defaultExchange = new Exchange(string.Empty);

        if (!string.IsNullOrEmpty(queueName))
        {
            var advanced = _bus.Advanced;
            var queue = await advanced.QueueDeclareAsync(queueName);
            await advanced.PublishAsync(
                exchange: defaultExchange,
                routingKey: queue.Name,
                mandatory: true,
                message: message);
        }
        else
        {
            await _bus.PubSub.PublishAsync(@event);
        }
    }

    public async Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage,
        CancellationToken cancellationToken) where T : class
    {
        Console.WriteLine($"Subscribing to event: {typeof(T).Name} with subscription ID: {subscriptionId}");
        await _bus.PubSub.SubscribeAsync(subscriptionId, onMessage, cancellationToken);
    }
}