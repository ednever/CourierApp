namespace API_server.Controllers
{
    // A background service responsible for periodically fetching and updating weather data
    public class WeatherUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private TimeSpan _updateInterval;
        private CancellationTokenSource _cts;
        private bool _isFirstRun;

        public WeatherUpdateService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _updateInterval = TimeSpan.FromMinutes(60); // Default update frequency set to every hour
            _cts = new CancellationTokenSource();
            _isFirstRun = true; // Initial run is scheduled for HH:15:00
        }

        // Executing the background task, periodically fetching weather data based on the configured interval
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
                        delay = CalculateDelayToNextRun(); // Schedule the first run at HH:15:00
                        _isFirstRun = false;
                    }
                    else
                    {
                        delay = _updateInterval; // Subsequent runs use the configured interval
                    }

                    // Wait for the calculated delay before fetching data
                    await Task.Delay(delay, linkedCts.Token); 

                    using var scope = _scopeFactory.CreateScope();
                    var weatherDataService = scope.ServiceProvider.GetRequiredService<IWeatherDataService>();
                    await weatherDataService.FetchAndSaveWeatherData();
                }
                catch (TaskCanceledException)
                {
                    continue; // Skip to the next iteration if the task is canceled
                }
            }
        }

        // The delay calculating until the next scheduled run, aligning the first run with HH:15:00.
        private TimeSpan CalculateDelayToNextRun()
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(now.Hour).AddMinutes(15);

            if (now > nextRun)
            {
                // Move to the next hour if the current time has passed HH:15:00
                nextRun = nextRun.AddHours(1);
            }

            return nextRun - now;
        }

        // A new update interval setting and restarting the scheduling process
        public void SetUpdateInterval(int minutes)
        {
            if (minutes <= 0)
            {
                throw new ArgumentException("Interval must be greater than 0 minutes.");
            }
            _updateInterval = TimeSpan.FromMinutes(minutes);
            _cts.Cancel(); // Cancel the current delay
            _cts.Dispose();
            _cts = new CancellationTokenSource(); // Create a new cancellation token source
            _isFirstRun = false; // After changing the interval, no longer align with HH:15:00
        }

        // Stopping the background service and disposing the cancellation token source
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            await base.StopAsync(cancellationToken);
            _cts.Dispose();
        }
    }
}