using System.Diagnostics;
using ArticleService.Model;
using EasyNetQ;
using Newtonsoft.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace ArticleService.Messaging;

public class MessageClient
{
    private readonly IBus _bus;

    public MessageClient(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T @event) where T : class
    {
        Console.WriteLine($"Publishing event: {typeof(T).Name} - {JsonConvert.SerializeObject(@event)}");
        await _bus.PubSub.PublishAsync(@event);
    }

    public async Task SubscribeAsync<T>(string queueName, Func<T, Task> onMessage) where T : class
    {
        using var advanced = _bus.Advanced;
        var queue = await advanced.QueueDeclareAsync(queueName);

        async Task Handler(IMessage<Message<T>> msg, MessageReceivedInfo info)
        {
            Console.WriteLine("Deliverytag: " + info.DeliveryTag);
            Console.WriteLine("Message Type: " + msg.Body.MessageType);

            var headers = msg.Properties.Headers;
            var propagator = new TraceContextPropagator();
            var parentContext = propagator.Extract(default, headers,
                (r, key) =>
                {
                    return new List<string>(new[] { r.ContainsKey(key) ? r[key].ToString() : String.Empty }!);
                });
            Baggage.Current = parentContext.Baggage;
            using var activity = MonitorService.MonitorService.ActivitySource.StartActivity("Consumed Article",
                ActivityKind.Consumer, parentContext.ActivityContext);

            await onMessage(msg.Body.Body);
        }

        advanced.Consume(queue, (Func<IMessage<Message<T>>, MessageReceivedInfo, Task>)Handler);
    }
}