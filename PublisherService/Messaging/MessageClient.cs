using EasyNetQ;
using EasyNetQ.Serialization.NewtonsoftJson;
using EasyNetQ.Topology;
using Newtonsoft.Json;
using PublisherService.Contracts;
using Serilog.Sinks.File;

namespace PublisherService.Messaging;

public class MessageClient
{
    private readonly IBus _bus;

    public MessageClient(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync(Article article, MessageProperties properties, string? queueName = null)
    {
        using var activity = MonitorService.MonitorService.ActivitySource.StartActivity("Publishing Article");
        
        var json = JsonConvert.SerializeObject(article);
        var message = new Message<string>(json, properties);

        var defaultExchange = new Exchange(string.Empty);

        var advanced = _bus.Advanced;
        var queue = await advanced.QueueDeclareAsync(queueName);
        await advanced.PublishAsync(
            exchange: defaultExchange,
            routingKey: queue.Name,
            mandatory: true,
            message: message);
    }

    public async Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage,
        CancellationToken cancellationToken) where T : class
    {
        Console.WriteLine($"Subscribing to event: {typeof(T).Name} with subscription ID: {subscriptionId}");
        await _bus.PubSub.SubscribeAsync(subscriptionId, onMessage, cancellationToken);
    }
}