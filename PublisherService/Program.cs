using EasyNetQ;
using OpenTelemetry.Trace;
using PublisherService;
using PublisherService.Messaging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<MessageClient>();
builder.Services.AddHostedService<PublisherBackgroundService>();

builder.Services.AddSingleton<IBus>(sp =>
{
    for (int i = 0; i < 10; i++)
    {
        try
        {
            var bus = RabbitHutch.CreateBus("host=rmq;virtualHost=/;username=guest;password=guest");
            Console.WriteLine($"EasyNetQ connected to rabbitmq on attempt {i+1}");
            return bus;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to RabbitMQ: {ex.Message}");
            Thread.Sleep(5000);
        }
    }

    throw new Exception("Could not connect to RabbitMQ after multiple attempts.");
});

Log.CloseAndFlush();
MonitorService.MonitorService.TracerProvider.ForceFlush();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();