using API_server.Data;
using API_server.Models;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Grabs all tariffs like a champ. Returns every city tariff in the house—no weak links here.
        /// </summary>
        /// <returns>An ActionResult packed with an enumerable of City objects, delivered with swagger</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> GetTariffs()
        {
            return await _context.City.ToListAsync();
        }

        /// <summary>
        /// Snags a tariff by ID with pure finesse. You ask, it delivers—unless that ID’s a ghost.
        /// </summary>
        /// <param name="id">The ID of the tariff you’re hunting—better be legit</param>
        /// <returns>An ActionResult with a City object if it’s found, or a NotFound if you’re out of luck</returns>
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

        /// <summary>
        /// Creates a new tariff like a boss. Drops it into the system with style and a victory lap.
        /// </summary>
        /// <param name="tariff">The City object—your new tariff, ready to roll in hot</param>
        /// <returns>An ActionResult with the created City, plus a slick redirect to prove it’s real</returns>
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

        /// <summary>
        /// Updates a tariff with swagger and precision. You bring the ID and the juice, it makes it happen.
        /// </summary>
        /// <param name="id">The ID of the tariff you’re flexing on—keep it tight</param>
        /// <param name="tariff">The City object with the fresh updates—don’t sleep on this</param>
        /// <returns>An IActionResult that’s either a smooth Ok or a BadRequest/NotFound if you slip</returns>
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

        /// <summary>
        /// Deletes a tariff like it’s nothing. Finds it, trashes it, and walks away cool as ever.
        /// </summary>
        /// <param name="id">The ID of the tariff you’re kicking to the curb—say goodbye</param>
        /// <returns>An IActionResult—NoContent if it’s gone, NotFound if it was never there</returns>
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
