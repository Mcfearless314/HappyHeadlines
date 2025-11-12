using PublisherService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<DbProvider>(sp => new DbProvider("Server=publisher-db;User Id=sa;Password=SuperSecret7!;TrustServerCertificate=True;"));
builder.Services.AddScoped<Database>();
builder.Services.AddScoped<DbInitializer>();

var app = builder.Build();

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