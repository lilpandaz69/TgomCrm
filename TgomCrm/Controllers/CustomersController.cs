using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;
using Tagom.Infrastructure.Persistence;
using Tagom.Application.DTOs;

namespace TgomCrm.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly TagomDbContext _db;
        public CustomersController(TagomDbContext db) => _db = db;

        // ✅ GET api/customers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _db.Customers
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email
                })
                .ToListAsync();

            return Ok(customers);
        }

        // ✅ GET api/customers/by-phone/{phone}
        [HttpGet("by-phone/{phone}")]
        public async Task<IActionResult> GetByPhone(string phone)
        {
            var customer = await _db.Customers
                .Where(c => c.Phone == phone)
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email
                })
                .FirstOrDefaultAsync();

            if (customer == null)
                return NotFound($"No customer found with phone number: {phone}");

            return Ok(customer);
        }

        // ✅ POST api/customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var customer = new Customer
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByPhone), new { phone = customer.Phone }, dto);
        }
    }
}
