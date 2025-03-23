using Microsoft.EntityFrameworkCore;
using API_server.Data;
using API_server.Controllers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure the database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=Data/MyDatabase.db"));

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Include XML documentation for Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath); // Enable XML comments for API documentation
});

// Register scoped services for dependency injection
builder.Services.AddScoped<IWeatherDataService, WeatherDataService>();
builder.Services.AddScoped<IWeatherUpdateFrequencyService, WeatherUpdateFrequencyService>();

// Register WeatherUpdateService as a singleton and hosted service
builder.Services.AddSingleton<WeatherUpdateService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<WeatherUpdateService>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Enable Swagger in development environment
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure middleware pipeline
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Apply database migrations on application startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
