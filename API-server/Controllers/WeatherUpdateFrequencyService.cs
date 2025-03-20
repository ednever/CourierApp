namespace API_server.Controllers
{
    public interface IWeatherUpdateFrequencyService
    {
        void SetUpdateFrequency(int minutes);
    }
    public class WeatherUpdateFrequencyService : IWeatherUpdateFrequencyService
    {
        private readonly WeatherUpdateService _weatherUpdateService;

        public WeatherUpdateFrequencyService(WeatherUpdateService weatherUpdateService)
        {
            _weatherUpdateService = weatherUpdateService;
        }

        public void SetUpdateFrequency(int minutes)
        {
            _weatherUpdateService.SetUpdateInterval(minutes);
        }
    }
}
