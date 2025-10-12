using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;
using Tagom.Infrastructure.Persistence;
using Tagom.Application.DTOs; // ✅ Use the external DTO

namespace TagomCrm.API.Controllers
{
    // ==============================
    // ✅ SUPPLIERS CONTROLLER
    // ==============================
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly TagomDbContext _db;
        public SuppliersController(TagomDbContext db) => _db = db;

        // ✅ GET all suppliers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _db.Suppliers
                .AsNoTracking()
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Phone,
                })
                .ToListAsync();

            return Ok(suppliers);
        }

        // ✅ GET supplier by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _db.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Phone,
                })
                .FirstOrDefaultAsync();

            if (supplier == null)
                return NotFound($"Supplier with ID {id} not found.");

            return Ok(supplier);
        }

        // ✅ POST create new supplier
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Supplier s)
        {
            if (string.IsNullOrWhiteSpace(s.Name))
                return BadRequest("Name is required.");

            _db.Suppliers.Add(s);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = s.Id }, new
            {
                s.Id,
                s.Name,
                s.Phone,
            });
        }
    }

    // ==============================
    // ✅ PRODUCTS CONTROLLER
    // ==============================
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly TagomDbContext _db;
        public ProductsController(TagomDbContext db) => _db = db;

        // ✅ GET all products (with supplier info)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _db.Products
                .Include(p => p.Supplier)
                .AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Category,
                    p.Stock,
                    Supplier = new
                    {
                        p.Supplier.Id,
                        p.Supplier.Name
                    }
                })
                .ToListAsync();

            return Ok(products);
        }

        // ✅ GET product by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _db.Products
                .Include(p => p.Supplier)
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Category,
                    p.Stock,
                    Supplier = new
                    {
                        p.Supplier.Id,
                        p.Supplier.Name
                    }
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            return Ok(product);
        }

        // ✅ POST create or update product stock
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            // ✅ Check supplier exists
            var supplier = await _db.Suppliers.FindAsync(dto.SupplierId);
            if (supplier == null)
                return BadRequest($"Supplier with ID {dto.SupplierId} not found.");

            // ✅ Check if product with same name and supplier already exists
            var existingProduct = await _db.Products
                .FirstOrDefaultAsync(p => p.Name.ToLower() == dto.Name.ToLower()
                                       && p.SupplierId == dto.SupplierId);

            if (existingProduct != null)
            {
                // ✅ Increase stock instead of creating duplicate
                existingProduct.Stock += dto.Stock;
                existingProduct.Price = dto.Price; // optional update
                existingProduct.Category = dto.Category;

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Stock updated for existing product '{existingProduct.Name}'.",
                    product = new
                    {
                        existingProduct.Id,
                        existingProduct.Name,
                        existingProduct.Stock,
                        existingProduct.Price,
                        existingProduct.Category,
                        Supplier = new
                        {
                            supplier.Id,
                            supplier.Name
                        }
                    }
                });
            }

            // ✅ Otherwise, create a new product
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Category = dto.Category,
                Stock = dto.Stock,
                SupplierId = dto.SupplierId
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new
            {
                message = "New product created successfully.",
                product = new
                {
                    product.Id,
                    product.Name,
                    product.Price,
                    product.Category,
                    product.Stock,
                    Supplier = new
                    {
                        supplier.Id,
                        supplier.Name
                    }
                }
            });
        }
    }
}
