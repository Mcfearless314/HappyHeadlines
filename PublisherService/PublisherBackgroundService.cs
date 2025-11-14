using System.Diagnostics;
using EasyNetQ;
using EasyNetQ.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using PublisherService.Contracts;
using PublisherService.Messaging;

namespace PublisherService;

public class PublisherBackgroundService : BackgroundService
{
    private readonly MessageClient _messageClient;
    private int _counter = 7;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public PublisherBackgroundService(MessageClient messageClient)
    {
        _messageClient = messageClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        MonitorService.MonitorService.Log.Debug("[PublisherService] Background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = MonitorService.MonitorService.ActivitySource.StartActivity();


            var article = new Article
            {
                Id = _counter,
                Title = $"This is article title {_counter}",
                Content = $"This is content {_counter}",
                PublishDate = DateTime.Today.AddDays(_counter)
            };
            

            var activityContext = activity?.Context ?? Activity.Current?.Context ?? default;
            var propagationContext = new PropagationContext(activityContext, Baggage.Current);
            var propagator = new TraceContextPropagator();

            var properties = new MessageProperties();

            propagator.Inject(
                propagationContext,
                properties,
                (props, key, value) =>
                {
                    props.Headers[key] = value;  
                });

            try
            {
                await _messageClient.PublishAsync(article, properties, queueName:"article-queue");
                MonitorService.MonitorService.Log.Debug($"Published article {_counter}");
                _counter++;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Publish task cancelled: {ex.Message}");
                Console.WriteLine($"Stacktrace:  {ex.StackTrace}");
                await Task.Delay(2000, stoppingToken); 
            }
            catch (Exception ex)
            {
                MonitorService.MonitorService.Log.Error(ex, "Failed to publish article. Retrying...");
                await Task.Delay(2000, stoppingToken);
            }
            
            await Task.Delay(_interval, stoppingToken);
        }
    }
}