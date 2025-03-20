using API_server.Data;
using API_server.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace API_server.Controllers
{
    public interface IWeatherDataService
    {
        Task FetchAndSaveWeatherData();
        Task<IEnumerable<Weather>> WeatherDataToList();
        // void SetUpdateFrequency(int minutes);
    }
    public class WeatherDataService : IWeatherDataService
    {
        private readonly AppDbContext _context;
        // private readonly WeatherUpdateService _weatherUpdateService;

        public WeatherDataService(AppDbContext context) //, WeatherUpdateService weatherUpdateService
        {
            _context = context;
            //_weatherUpdateService = weatherUpdateService;
        }

        public async Task FetchAndSaveWeatherData()
        {
            // Список станций, которые нас интересуют
            var targetStations = new[] { "Tallinn-Harku", "Tartu-Tõravere", "Pärnu" };

            // Загружаем XML с помощью HttpClient
            using var client = new HttpClient();
            var xmlString = await client.GetStringAsync("https://www.ilmateenistus.ee/ilma_andmed/xml/observations.php");

            // Парсим XML
            var doc = XDocument.Parse(xmlString);
            var timestamp = long.Parse(doc.Root.Attribute("timestamp").Value);

            // Извлекаем данные для нужных станций
            var weatherData = doc.Descendants("station")
                .Where(s => targetStations.Contains(s.Element("name")?.Value))
                .Select(s => new
                {
                    StationName = s.Element("name")?.Value,
                    WMOCode = int.Parse(s.Element("wmocode")?.Value ?? "0"),
                    AirTemperature = decimal.Parse(s.Element("airtemperature")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                    WindSpeed = decimal.Parse(s.Element("windspeed")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                    Phenomenon = s.Element("phenomenon")?.Value,
                })
                .ToList();

            foreach (var data in weatherData)
            {
                // Проверяем, существует ли явление в базе, или добавляем новое
                var phenomenon = _context.Phenomenon.FirstOrDefault(p => p.Name == data.Phenomenon);
                if (phenomenon == null && !string.IsNullOrEmpty(data.Phenomenon))
                {
                    phenomenon = new Phenomenon { Name = data.Phenomenon };
                    _context.Phenomenon.Add(phenomenon);
                    await _context.SaveChangesAsync();
                }

                // Добавляем погодные данные
                var weather = new Weather
                {
                    StationName = data.StationName,
                    WMOCode = data.WMOCode,
                    AirTemperature = data.AirTemperature,
                    WindSpeed = data.WindSpeed,
                    PhenomenonID = phenomenon.ID,
                    Timestamp = (int)timestamp
                };
                _context.Weather.Add(weather);
            }

            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Weather>> WeatherDataToList()
        {
            return await _context.Weather.ToListAsync();
        }
        // Метод для изменения частоты обновления
        //public void SetUpdateFrequency(int minutes)
        //{
        //     _weatherUpdateService.SetUpdateInterval(minutes);
        //}
    }
}
