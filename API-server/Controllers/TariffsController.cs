using API_server.Data;
using API_server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TariffsController : Controller
    {
        private readonly AppDbContext _context;

        public TariffsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tariffs - Получить все тарифы
        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> GetTariffs()
        {
            return await _context.City.ToListAsync();
        }

        // GET: api/Tariffs/5 - Получить тариф по ID
        [HttpGet("{id}")]
        public async Task<ActionResult<City>> GetTariff(int id)
        {
            var tariff = await _context.City.FirstOrDefaultAsync(t => t.ID == id);

            if (tariff == null)
            {
                return NotFound();
            }

            return tariff;
        }

        // POST: api/Tariffs - Создать новый тариф
        [HttpPost]
        public async Task<ActionResult<City>> CreateTariff([FromBody] City tariff)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.City.Add(tariff);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTariff), new { id = tariff.ID }, tariff);
        }

        // PUT: api/Tariffs/5 - Обновить существующий тариф
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTariff(int id, [FromBody] City tariff)
        {
            if (id != tariff.ID)
            {
                return BadRequest("ID mismatch");
            }

            var existingTariff = await _context.City.FindAsync(id);
            if (existingTariff == null)
            {
                return NotFound();
            }

            existingTariff.Name = tariff.Name;
            existingTariff.PriceForCar = tariff.PriceForCar;
            existingTariff.PriceForScooter = tariff.PriceForScooter;
            existingTariff.PriceForBicycle = tariff.PriceForBicycle;

            await _context.SaveChangesAsync();            
            return Ok();
        }

        // DELETE: api/Tariffs/5 - Удалить тариф
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTariff(int id)
        {
            var tariff = await _context.City.FindAsync(id);
            if (tariff == null)
            {
                return NotFound();
            }

            _context.City.Remove(tariff);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
