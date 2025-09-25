using ArticleService;
using ArticleService.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<DbProvider>(sp => new DbProvider("Server=article-db;User Id=sa;Password=SuperSecret7!;TrustServerCertificate=True;"));
builder.Services.AddScoped<Database>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ArticleCache>();

var app = builder.Build();

// Initialize DB when the app starts
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    initializer.Initialize();
}

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();
