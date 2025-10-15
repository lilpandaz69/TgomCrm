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
    public class InventoryController : ControllerBase
    {
        private readonly TagomDbContext _db;
        public InventoryController(TagomDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var inventoryData = await _db.Inventories
                .Include(i => i.Product)
                .ThenInclude(p => p.Supplier)
                .AsNoTracking()
                .GroupBy(i => i.Product.Category)
                .Select(categoryGroup => new
                {
                    Category = categoryGroup.Key,
                    Products = categoryGroup.GroupBy(x => new { x.Product.ProductId, x.Product.Name })
                        .Select(productGroup => new
                        {
                            ProductId = productGroup.Key.ProductId,
                            ProductName = productGroup.Key.Name,
                            TotalStock = productGroup.Sum(x => x.Quantity),
                            Suppliers = productGroup.Select(x => new
                            {
                                SupplierId = x.Product.Supplier.SupplierId,
                                SupplierName = x.Product.Supplier.Name,
                                StockFromSupplier = x.Quantity
                            }).ToList()
                        }).ToList()
                })
                .ToListAsync();

            return Ok(inventoryData);
        }
    }
}
