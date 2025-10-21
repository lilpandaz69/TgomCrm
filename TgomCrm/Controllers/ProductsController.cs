using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Tagom.Domain.Entities;
using Tagom.Infrastructure.Persistence;
using Tagom.Application.DTOs;

namespace TgomCrm.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly TagomDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(TagomDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpPut("{id}/update")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductDto dto)
        {
            var product = await _db.Products.Include(p => p.Inventory).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            var supplier = await _db.Suppliers.FindAsync(dto.SupplierId);
            if (supplier == null)
                return BadRequest($"Supplier with ID {dto.SupplierId} not found.");

            string? imagePath = product.ImageUrl;

            
            if (dto.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "images"
                );

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                imagePath = $"/images/{fileName}";
            }

            
            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Orignailprice = dto.Orignailprice;
            product.Category = dto.Category;
            product.Stock = dto.Stock;
            product.SupplierId = dto.SupplierId;
            product.ImageUrl = imagePath;

            if (product.Inventory != null)
                product.Inventory.Quantity = dto.Stock;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Product '{product.Name}' updated successfully.",
                product = new
                {
                    product.ProductId,
                    product.Name,
                    product.Price,
                    product.Orignailprice,
                    product.Category,
                    product.Stock,
                    product.ImageUrl,
                    Supplier = new { supplier.SupplierId, supplier.Name }
                }
            });
        }

        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.Include(p => p.Inventory).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            
            if (product.Inventory != null)
                _db.Inventories.Remove(product.Inventory);

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Product '{product.Name}' deleted successfully." });
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _db.Products
                .Include(p => p.Supplier)
                .AsNoTracking()
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Price,
                    p.Category,
                    p.Stock,
                    p.ImageUrl,
                    Supplier = new { p.Supplier.SupplierId, p.Supplier.Name }
                })
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _db.Products
                .Include(p => p.Supplier)
                .AsNoTracking()
                .Where(p => p.ProductId == id)
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Price,
                    p.Category,
                    p.Stock,
                    p.ImageUrl,
                    Supplier = new { p.Supplier.SupplierId, p.Supplier.Name }
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductDto dto)
        {
            var supplier = await _db.Suppliers.FindAsync(dto.SupplierId);
            if (supplier == null)
                return BadRequest($"Supplier with ID {dto.SupplierId} not found.");

            var existingProduct = await _db.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Name.ToLower() == dto.Name.ToLower() && p.SupplierId == dto.SupplierId);

            string? imagePath = null;

            // 🖼️ Handle image upload
            if (dto.ImageFile != null)
            {
                var uploadsFolder = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "images"
                );

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                imagePath = $"/images/{fileName}";
            }

            if (existingProduct != null)
            {
                existingProduct.Stock += dto.Stock;
                existingProduct.Price = dto.Price;
                existingProduct.Category = dto.Category;

                if (imagePath != null)
                    existingProduct.ImageUrl = imagePath;

                if (existingProduct.Inventory == null)
                    existingProduct.Inventory = new Inventory { ProductId = existingProduct.ProductId, Quantity = existingProduct.Stock };
                else
                    existingProduct.Inventory.Quantity += dto.Stock;

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Stock updated for existing product '{existingProduct.Name}'.",
                    product = new
                    {
                        existingProduct.ProductId,
                        existingProduct.Name,
                        existingProduct.Stock,
                        existingProduct.Price,
                        existingProduct.Orignailprice,
                        existingProduct.Category,
                        existingProduct.ImageUrl,
                        Supplier = new { supplier.SupplierId, supplier.Name }
                    }
                });
            }

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Orignailprice = dto.Orignailprice,
                Category = dto.Category,
                Stock = dto.Stock,
                SupplierId = dto.SupplierId,
                Inventory = new Inventory { Quantity = dto.Stock },
                ImageUrl = imagePath
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, new
            {
                message = "New product created successfully with inventory record.",
                product = new
                {
                    product.ProductId,
                    product.Name,
                    product.Price,
                    product.Orignailprice,
                    product.Category,
                    product.Stock,
                    product.ImageUrl,
                    Supplier = new { supplier.SupplierId, supplier.Name }
                }
            });

        }

    }
}
