using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Tagom.Domain.Entities;
using Tagom.Infrastructure.Persistence;
using Tagom.Application.DTOs;

namespace TgomCrm.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
        public class SuppliersController : ControllerBase
        {
            private readonly TagomDbContext _db;
            public SuppliersController(TagomDbContext db) => _db = db;

            [HttpGet]
            public async Task<IActionResult> GetAll()
            {
                var suppliers = await _db.Suppliers
                    .AsNoTracking()
                    .Select(s => new { s.SupplierId, s.Name, s.Phone })
                    .ToListAsync();
                return Ok(suppliers);
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> GetById(int id)
            {
                var supplier = await _db.Suppliers
                    .AsNoTracking()
                    .Where(s => s.SupplierId == id)
                    .Select(s => new { s.SupplierId, s.Name, s.Phone })
                    .FirstOrDefaultAsync();

                if (supplier == null)
                    return NotFound($"Supplier with ID {id} not found.");

                return Ok(supplier);
            }

            [HttpPost]
            public async Task<IActionResult> Create([FromBody] Supplier s)
            {
                if (string.IsNullOrWhiteSpace(s.Name))
                    return BadRequest("Name is required.");

                _db.Suppliers.Add(s);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = s.SupplierId }, new { s.SupplierId, s.Name, s.Phone });
            }
        }
}
