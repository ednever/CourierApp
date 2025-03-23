using API_server.Data;
using API_server.Models;
using Microsoft.AspNetCore.Mvc;

namespace API_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DeliveryController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculates delivery cost like a total rockstar. Takes your request and spits out the slickest price, factoring in weather and vibes.
        /// </summary>
        /// <param name="request">The DeliveryRequest object—your VIP pass with city and transport details, no posers allowed</param>
        /// <returns>An ActionResult with a DeliveryResponse that’s either dripping with success or a BadRequest if you fumble the bag</returns>
        [HttpPost("calculate")]
        public ActionResult<DeliveryResponse> CalculateCost([FromBody] DeliveryRequest request)
        {
            // Validation of input parameters
            var validCities = new[] { "Tallinn-Harku", "Tartu-Tõravere", "Pärnu" };
            var validTransports = new[] { "Car", "Scooter", "Bicycle" };

            if (!validCities.Contains(request.City) || !validTransports.Contains(request.Transport))
            {
                return BadRequest("Invalid city or transport");
            }

            // Retrieve base cost from the City table
            var city = _context.City
                .FirstOrDefault(c => c.Name == request.City);
            if (city == null)
            {
                return BadRequest("City not found in database.");
            }

            // Determine base cost based on transport type
            decimal baseCost = request.Transport switch
            {
                "Car" => city.PriceForCar,
                "Scooter" => city.PriceForScooter,
                "Bicycle" => city.PriceForBicycle,
                _ => 0m
            };

            decimal weatherSurcharge = 0m;

            // Retrieve the latest weather data for the city
            var latestWeather = _context.Weather
                .Where(w => w.StationName == request.City)
                .OrderByDescending(w => w.Timestamp)
                .FirstOrDefault();

            if (latestWeather != null)
            {

                // Check weather conditions for scooters and bicycles
                if (request.Transport == "Scooter" || request.Transport == "Bicycle")
                {
                    // Check temperature (ATEF)
                    decimal temperature = latestWeather.AirTemperature;
                    if (temperature < -10m)
                    {
                        weatherSurcharge = 1m; // Add 1€ surcharge for temperatures below -10°C
                    }
                    else if (temperature >= -10m && temperature <= 0m)
                    {
                        weatherSurcharge = 0.5m; // Add 0.5€ surcharge for temperatures between -10°C and 0°C
                    }

                    // Check weather phenomena (WPEF)
                    var phenomenon = _context.Phenomenon
                        .FirstOrDefault(p => p.ID == latestWeather.PhenomenonID);
                    if (phenomenon != null)
                    {
                        string phenomenonName = phenomenon?.Name?.ToLower();
                        if (!string.IsNullOrEmpty(phenomenonName))
                        {
                            var snowPhenomena = new[] { "light snow shower", "moderate snow shower", "heavy snow shower", "light sleet", "moderate sleet", "light snowfall", "moderate snowfall", "heavy snowfall" };
                            var rainPhenomena = new[] { "light shower", "moderate shower", "heavy shower", "light rain", "moderate rain", "heavy rain" };
                            var prohibitedPhenomena = new[] { "glaze", "hail" };
                            if (prohibitedPhenomena.Contains(phenomenonName))
                            {
                                return BadRequest($"{request.Transport} delivery is prohibited due to {phenomenonName}.");
                            }
                            else if (snowPhenomena.Contains(phenomenonName))
                            {
                                weatherSurcharge += 1m; // Add 1€ surcharge for snow-related phenomena
                            }
                            else if (rainPhenomena.Contains(phenomenonName))
                            {
                                weatherSurcharge += 0.5m; // Add 0.5€ surcharge for rain-related phenomena
                            }
                        }
                    }
                }

                // Check wind speed for bicycles only (WSEF)
                if (request.Transport == "Bicycle")
                {
                    decimal windSpeed = latestWeather.WindSpeed;
                    if (windSpeed > 20m)
                    {
                        return BadRequest("Bicycle delivery is prohibited due to wind speed exceeding 20 m/s.");
                    }
                    else if (windSpeed >= 10m && windSpeed <= 20m)
                    {
                        weatherSurcharge += 0.5m; // Add 0.5€ surcharge for wind speeds between 10 and 20 m/s
                    }
                }
            }

            // Calculate total cost
            decimal totalCost = baseCost + weatherSurcharge;

            // Construct response
            var response = new DeliveryResponse
            {
                Message = $"You chose delivery in {request.City} using {request.Transport}. " +
                  (weatherSurcharge > 0 ? $"Weather surcharge applied: {weatherSurcharge}€." : ""),
                Cost = totalCost
            };

            return Ok(response);
        }
    }
}
