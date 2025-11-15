using System.Diagnostics;
using System.Text;
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

        advanced.Consume<string>(queue, async (msg, info) =>
        {
            Console.WriteLine("Recieved raw JSON message");

            var headers = msg.Properties.Headers;
            var propagator = new TraceContextPropagator();
            
            IEnumerable<string> Getter(IDictionary<string, object> carrier, string key)
            {
                if (carrier.TryGetValue(key, out var value) && value != null)
                {
                    if (value is byte[] bytes)
                    {
                        var s = Encoding.UTF8.GetString(bytes);
                        Console.WriteLine($"Header {key}: {s}");
                        return new[] { s };
                    }

                    Console.WriteLine($"Header {key}: {value}");
                    return new[] { value.ToString()! };
                }
                
                return Array.Empty<string>();
            }
            
            var parentContext = propagator.Extract(
                default,
                headers,
                Getter
            );

            Baggage.Current = parentContext.Baggage;
            using var activity = MonitorService.MonitorService.ActivitySource.StartActivity("Consumed Article",
                ActivityKind.Consumer, parentContext.ActivityContext);

            var obj = JsonConvert.DeserializeObject<T>(msg.Body);
            if (obj == null)
                throw new InvalidOperationException("Failed to deserialize JSON to type " + typeof(T).Name);

            await onMessage(obj);
        });
    }
}