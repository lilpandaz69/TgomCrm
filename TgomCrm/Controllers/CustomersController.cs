using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET api/customers?search=&sort=newest|oldest&page=1&pageSize=8
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string sort = "newest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 8)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 8;

            var q = _db.Customers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(c =>
                    c.Name.ToLower().Contains(s) ||
                    (c.Phone ?? "").ToLower().Contains(s) ||
                    (c.Email ?? "").ToLower().Contains(s));
            }

            // إجمالي العدد قبل الباجينج
            var total = await q.CountAsync();

            // الفرز
            q = sort == "oldest"
                ? q.OrderBy(c => c.CustomerId)
                : q.OrderByDescending(c => c.CustomerId);

            // الباجينج
            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerDto
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Phone = c.Phone,
                    Email = c.Email
                })
                .ToListAsync();

            return Ok(new CustomersListResponse
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        // GET api/customers/by-phone/{phone}
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

        // POST api/customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var entity = new Tagom.Domain.Entities.Customer
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email
            };
            _db.Customers.Add(entity);
            await _db.SaveChangesAsync();

            var createdDto = new CustomerDto
            {
                CustomerId = entity.CustomerId,
                Name = entity.Name,
                Phone = entity.Phone,
                Email = entity.Email
            };

            return CreatedAtAction(nameof(GetByPhone), new { phone = entity.Phone }, createdDto);
        }
    }
}
