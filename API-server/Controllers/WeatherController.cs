using API_server.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        // Separate service for working with weather data
        private readonly IWeatherDataService _weatherDataService;
        // Separate service for working with data update frequency
        private readonly IWeatherUpdateFrequencyService _weatherUpdateFrequencyService;

        public WeatherController(IWeatherDataService weatherDataService, IWeatherUpdateFrequencyService weatherUpdateFrequencyService)
        {
            _weatherDataService = weatherDataService;
            _weatherUpdateFrequencyService = weatherUpdateFrequencyService;
        }

        /// <summary>
        /// Gets all the weather data like a boss. Returns a list of weather info that'll blow your mind.
        /// </summary>
        /// <returns>A slick ActionResult containing an enumerable of Weather objects, served up fresh</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Weather>>> GetWeatherData()
        {
            return Ok(await _weatherDataService.WeatherDataToList());
        }

        /// <summary>
        /// Loads weather data with style and swagger. Fetches and saves it like a pro.
        /// </summary>
        /// <returns>An ActionResult with a Weather object, delivered with finesse</returns>
        [HttpPost]
        public async Task<ActionResult<Weather>> LoadWeatherData()
        {
            await _weatherDataService.FetchAndSaveWeatherData();
            return Ok();
        }

        /// <summary>
        /// Sets the update frequency with some serious attitude. You tell it how often, it makes it happen.
        /// </summary>
        /// <param name="minutes">The frequency in minutes, because you’re too cool for anything else</param>
        /// <returns>An ActionResult that’s either a smooth success message or a BadRequest if you mess it up</returns>
        [HttpPost("setfrequency")]
        public ActionResult SetFrequency([FromQuery] int minutes)
        {
            try
            {
                _weatherUpdateFrequencyService.SetUpdateFrequency(minutes);
                return Ok(new { Message = $"Update frequency set to {minutes} minutes." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest();
            }
        }
    }
}
