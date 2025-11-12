using ProfanityService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var connectionString = "Server=profanity-db;User Id=sa;Password=SuperSecret7!;TrustServerCertificate=True;";

builder.Services.AddSingleton(new DbProvider(connectionString));
builder.Services.AddScoped<Database>();
builder.Services.AddScoped<DbInitializer>();

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

