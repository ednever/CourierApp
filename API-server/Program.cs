using Microsoft.EntityFrameworkCore;
using API_server.Data;
using API_server.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=Data/MyDatabase.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// BackgroundService registration 
builder.Services.AddScoped<IWeatherDataService, WeatherDataService>();
builder.Services.AddScoped<IWeatherUpdateFrequencyService, WeatherUpdateFrequencyService>();
//builder.Services.AddHostedService<WeatherUpdateService>();

// Регистрация WeatherUpdateService как singleton для DI
builder.Services.AddSingleton<WeatherUpdateService>();

// Регистрация WeatherUpdateService как hosted service (использует тот же singleton)
builder.Services.AddHostedService(provider => provider.GetRequiredService<WeatherUpdateService>());


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
