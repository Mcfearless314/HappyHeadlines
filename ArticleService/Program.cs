using ArticleService;
using ArticleService.Infrastructure;
using ArticleService.Messaging;
using EasyNetQ;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<DbProvider>(sp => new DbProvider("Server=article-db;User Id=sa;Password=SuperSecret7!;TrustServerCertificate=True;"));
builder.Services.AddScoped<Database>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ArticleCache>();
//builder.Services.AddHostedService<ArticleBackgroundService>();
//builder.Services.AddHostedService<ArticleCacheBackgroundService>();
builder.Services.AddHostedService<ArticleQueueBackgroundService>();
builder.Services.AddScoped<MessageClient>();

builder.Services.AddSingleton<IBus>(sp =>
{
    for (int i = 0; i < 10; i++)
    {
        try
        {
            var bus = RabbitHutch.CreateBus("host=rmq;virtualHost=/;username=guest;password=guest");
            MonitorService.MonitorService.Log.Debug($"EasyNetQ connected to rabbitmq on attempt {i + 1}");
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

// Initialize DB when the app starts
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    initializer.Initialize();
}

app.MapControllers();

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapMetrics(); // Exposes /metrics
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();