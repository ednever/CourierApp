using API_server.Data;
using API_server.Models;
using System.Xml.Linq;

namespace API_server.Controllers
{
    public class WeatherUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private TimeSpan _updateInterval;
        private CancellationTokenSource _cts;
        private bool _isFirstRun; // Флаг для первого запуска

        public WeatherUpdateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _updateInterval = TimeSpan.FromMinutes(60); // Начальная частота - каждый час
            _cts = new CancellationTokenSource();
            _isFirstRun = true; // Первый запуск привязан к HH:15:00
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cts.Token);

                    TimeSpan delay;
                    if (_isFirstRun)
                    {
                        delay = CalculateDelayToNextRun(); // Первый запуск в HH:15:00
                        _isFirstRun = false;
                    }
                    else
                    {
                        delay = _updateInterval; // Последующие запуски с заданным интервалом
                    }

                    // Ждем фиксированный интервал
                    await Task.Delay(delay, linkedCts.Token); 

                    using var scope = _scopeFactory.CreateScope();
                    var weatherDataService = scope.ServiceProvider.GetRequiredService<IWeatherDataService>();
                    await weatherDataService.FetchAndSaveWeatherData();
                }
                catch (TaskCanceledException)
                {
                    continue;
                }
            }
        }

        // Метод для расчета времени до следующего запуска
        private TimeSpan CalculateDelayToNextRun()
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(now.Hour).AddMinutes(15);

            if (now > nextRun)
            {
                nextRun = nextRun.AddHours(1);
            }

            return nextRun - now;
        }

        // Метод для изменения интервала
        public void SetUpdateInterval(int minutes)
        {
            if (minutes <= 0)
            {
                throw new ArgumentException("Interval must be greater than 0 minutes.");
            }
            _updateInterval = TimeSpan.FromMinutes(minutes);
            _cts.Cancel();
            _cts.Dispose();
            _cts = new CancellationTokenSource();
            _isFirstRun = false; // После изменения интервала больше не привязываемся к HH:15:00
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            await base.StopAsync(cancellationToken);
            _cts.Dispose();
        }
    }
}