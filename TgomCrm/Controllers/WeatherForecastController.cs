using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;
using Tagom.Infrastructure.Persistence;

namespace TgomCrm.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly TagomDbContext _db;
        public CustomersController(TagomDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
            => Ok(await _db.Customers.AsNoTracking().ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var c = await _db.Customers.FindAsync(id);
            return c is null ? NotFound() : Ok(c);
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> Create([FromBody] Customer c)
        {
            if (string.IsNullOrWhiteSpace(c.Name))
                return BadRequest("Name is required.");

            _db.Customers.Add(c);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
        }
    }
}
