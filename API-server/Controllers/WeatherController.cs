using API_server.Data;
using API_server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;


namespace API_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherDataService _weatherDataService;
        private readonly IWeatherUpdateFrequencyService _weatherUpdateFrequencyService;

        public WeatherController(IWeatherDataService weatherDataService, IWeatherUpdateFrequencyService weatherUpdateFrequencyService)
        {
            _weatherDataService = weatherDataService;
            _weatherUpdateFrequencyService = weatherUpdateFrequencyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Weather>>> GetWeatherData()
        {
            return Ok(await _weatherDataService.WeatherDataToList());
        }

        [HttpPost]
        public async Task<ActionResult<Weather>> LoadWeatherData()
        {
            await _weatherDataService.FetchAndSaveWeatherData();
            return Ok();
        }

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
