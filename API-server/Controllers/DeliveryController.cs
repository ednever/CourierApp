using API_server.Data;
using API_server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

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

        [HttpPost("calculate")]
        public ActionResult<DeliveryResponse> CalculateCost([FromBody] DeliveryRequest request)
        {
            // Валидация входных параметров
            var validCities = new[] { "Tallinn-Harku", "Tartu-Tõravere", "Pärnu" };
            var validTransports = new[] { "Car", "Scooter", "Bicycle" };

            if (!validCities.Contains(request.City) || !validTransports.Contains(request.Transport))
            {
                return BadRequest("Invalid city or transport");
            }

            // Получаем базовую стоимость из таблицы City
            var city = _context.City
                .FirstOrDefault(c => c.Name == request.City);
            if (city == null)
            {
                return BadRequest("City not found in database.");
            }

            decimal baseCost = request.Transport switch
            {
                "Car" => city.PriceForCar,
                "Scooter" => city.PriceForScooter,
                "Bicycle" => city.PriceForBicycle,
                _ => 0m
            };

            decimal weatherSurcharge = 0m;

            // Получаем последнюю запись о погоде для города
            var latestWeather = _context.Weather
                .Where(w => w.StationName == request.City)
                .OrderByDescending(w => w.Timestamp)
                .FirstOrDefault();

            if (latestWeather != null)
            {

                // Проверяем погоду для самокатов и велосипедов
                if (request.Transport == "Scooter" || request.Transport == "Bicycle")
                {
                    decimal temperature = latestWeather.AirTemperature;
                    if (temperature < -10m)
                    {
                        weatherSurcharge = 1m; // Доплата 1€ при температуре ниже -10°C
                    }
                    else if (temperature >= -10m && temperature <= 0m)
                    {
                        weatherSurcharge = 0.5m; // Доплата 0.5€ при температуре от -10°C до 0°C
                    }                
                }

                // Проверка скорости ветра только для велосипедов
                if (request.Transport == "Bicycle")
                {
                    decimal windSpeed = latestWeather.WindSpeed;
                    if (windSpeed > 20m)
                    {
                        return BadRequest("Bicycle delivery is prohibited due to wind speed exceeding 20 m/s.");
                    }
                    else if (windSpeed >= 10m && windSpeed <= 20m)
                    {
                        weatherSurcharge += 0.5m; // Доплата 0.5€ при скорости ветра от 10 до 20 м/с
                    }
                }
            }

            decimal totalCost = baseCost + weatherSurcharge;

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
