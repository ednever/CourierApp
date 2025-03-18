using API_server.Data;
using API_server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;


namespace API_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : Controller
    {
        private readonly AppDbContext _context;
        public WeatherController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Weather>>> GetWeatherData()
        {
            return await _context.Weather.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<Weather>>> LoadWeatherData()
        {
            //_context.Weather.Add(weather);
            //await _context.SaveChangesAsync();
            //return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            await FetchAndSaveWeatherData();
            return await _context.Weather.ToListAsync();
        }

        private async Task FetchAndSaveWeatherData()
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
    }
}
