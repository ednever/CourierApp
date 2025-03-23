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
    }
    public class WeatherDataService : IWeatherDataService
    {
        private readonly AppDbContext _context;

        public WeatherDataService(AppDbContext context)
        {
            _context = context;
        }
        // Weather data fetching from ilmateenistus.ee XML source and saving it to the database
        public async Task FetchAndSaveWeatherData()
        {
            // List of stations to filter weather data for
            var targetStations = new[] { "Tallinn-Harku", "Tartu-Tõravere", "Pärnu" };

            // Load XML data using HttpClient
            using var client = new HttpClient();
            var xmlString = await client.GetStringAsync("https://www.ilmateenistus.ee/ilma_andmed/xml/observations.php");

            // Parse the XML document
            var doc = XDocument.Parse(xmlString);
            var timestamp = long.Parse(doc.Root.Attribute("timestamp").Value);

            // Extract weather data for the specified stations
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
                // Check if the phenomenon exists in the database; if not, add it
                var phenomenon = _context.Phenomenon.FirstOrDefault(p => p.Name == data.Phenomenon);
                if (phenomenon == null && !string.IsNullOrEmpty(data.Phenomenon))
                {
                    phenomenon = new Phenomenon { Name = data.Phenomenon };
                    _context.Phenomenon.Add(phenomenon);
                    await _context.SaveChangesAsync();
                }

                // Create and add weather data entry
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

            // Save all changes to the database
            await _context.SaveChangesAsync();
        }

        // Retrieving all weather data from the database and including related phenomenon information
        public async Task<IEnumerable<Weather>> WeatherDataToList()
        {
            var weatherData = await _context.Weather.ToListAsync();
            foreach (var weather in weatherData)
            {
                weather.Phenomenon = await _context.Phenomenon.FirstOrDefaultAsync(p => p.ID == weather.PhenomenonID);
            }
            return weatherData;
        }
    }
}
