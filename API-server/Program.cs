using Microsoft.EntityFrameworkCore;
using API_server.Data;
using API_server.Controllers;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Настройка базы данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=Data/MyDatabase.db"));

// Настройка конфигурации
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath); // Подключаем XML-документацию
});

// BackgroundService registration 
builder.Services.AddScoped<IWeatherDataService, WeatherDataService>();
builder.Services.AddScoped<IWeatherUpdateFrequencyService, WeatherUpdateFrequencyService>();

// Регистрация WeatherUpdateService
builder.Services.AddSingleton<WeatherUpdateService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<WeatherUpdateService>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Конфигурация middleware
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Применение миграций при старте
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
