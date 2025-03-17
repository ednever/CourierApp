using API_server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace API_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        [HttpPost("calculate")]
        public ActionResult<DeliveryResponse> CalculateCost([FromBody] DeliveryRequest request)
        {
            // Валидация входных параметров
            var validCities = new[] { "Tallinn", "Tartu", "Pärnu" };
            var validTransports = new[] { "Car", "Scooter", "Bicycle" };

            if (!validCities.Contains(request.City))
            {
                return BadRequest("Invalid city. Choose from: Tallinn, Tartu, Pärnu.");
            }

            if (!validTransports.Contains(request.Transport))
            {
                return BadRequest("Invalid transport. Choose from: Car, Scooter, Bicycle.");
            }

            decimal cost = 0;
            if (request.City == "Tallinn")
            {
                if (request.Transport == "Car") cost = 5.0m;
                else if (request.Transport == "Scooter") cost = 3.0m;
                else if (request.Transport == "Bicycle") cost = 2.0m;
            }
            else if (request.City == "Tartu")
            {
                if (request.Transport == "Car") cost = 4.5m;
                else if (request.Transport == "Scooter") cost = 2.5m;
                else if (request.Transport == "Bicycle") cost = 1.5m;
            }
            else if (request.City == "Pärnu")
            {
                if (request.Transport == "Car") cost = 4.0m;
                else if (request.Transport == "Scooter") cost = 2.0m;
                else if (request.Transport == "Bicycle") cost = 1.0m;
            }

            var response = new DeliveryResponse
            {
                Message = $"You chose delivery in {request.City} using {request.Transport}.",
                Cost = cost
            };

            return Ok(response);
        }
    }
}
