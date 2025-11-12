using CommentService;
using CommentService.Infrastructure;
using Polly;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<DbProvider>(sp => new DbProvider("Server=comment-db;User Id=sa;Password=SuperSecret7!;TrustServerCertificate=True;"));
builder.Services.AddScoped<Database>();
builder.Services.AddScoped<CommentCache>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddHostedService<CommentCacheBackgroundService>();

builder.Services.AddHttpClient("ProfanityService", client =>
    {
        client.BaseAddress = new Uri("http://profanity-service:8080");
        client.Timeout = TimeSpan.FromSeconds(5);
    })
    .AddPolicyHandler(_ =>
    {
        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .RetryAsync(3, (exception, retryCount) =>
            {
                Console.WriteLine($"Retrying due to {exception.GetType().Name}... Attempt {retryCount}");
            });

        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

        return retryPolicy.WrapAsync(circuitBreakerPolicy);
    });

var app = builder.Build();

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
