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
    public class SalesController : ControllerBase
    {
        private readonly TagomDbContext _db;
        public SalesController(TagomDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _db.Sales
                .Include(s => s.Customer)
                .Include(s => s.Product)
                .ThenInclude(p => p.Supplier)
                .AsNoTracking()
                .Select(s => new
                {
                    s.SaleId,
                    s.CustomerPhone,
                    Customer = new
                    {
                        s.Customer.CustomerId,
                        s.Customer.Name,
                        s.Customer.Email,
                        s.Customer.Phone
                    },
                    Product = new
                    {
                        s.Product.ProductId,
                        s.Product.Name,
                        Supplier = s.Product.Supplier != null
                            ? new { s.Product.Supplier.SupplierId, s.Product.Supplier.Name }
                            : null
                    },
                    s.UnitPrice,
                    s.Quantity,
                    s.TotalPrice,
                    s.SaleDate,
                    s.InvoiceId,
                    s.IsReturned,
                    s.ReturnReason
                })
                .ToListAsync();

            return Ok(sales);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaleDto dto)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Phone == dto.CustomerPhone);
            if (customer == null)
                return BadRequest("Customer not found.");

            var product = await _db.Products.Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId);
            if (product == null)
                return BadRequest("Product not found.");

            if (product.Stock < dto.Quantity)
                return BadRequest($"Not enough stock for '{product.Name}'.");

            var invoice = new Invoice { CustomerId = customer.CustomerId, SaleDate = DateTime.UtcNow };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            var sale = new Sale
            {
                CustomerPhone = dto.CustomerPhone,
                CustomerId = customer.CustomerId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * dto.Quantity,
                SaleDate = DateTime.UtcNow,
                InvoiceId = invoice.InvoiceId,
                IsReturned = false
            };

            product.Stock -= dto.Quantity;
            var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == product.ProductId);
            if (inventory != null)
            {
                inventory.Quantity -= dto.Quantity;
            }

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Sale created successfully!",
                sale.SaleId,
                sale.InvoiceId,
                sale.TotalPrice
            });
        }

        // ✅ RETURN SALE (REGURGITATION)
        [HttpPost("return/{saleId}")]
        public async Task<IActionResult> ReturnSale(int saleId, [FromBody] ReturnDto dto)
        {
            var sale = await _db.Sales
                .Include(s => s.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.SaleId == saleId);

            if (sale == null)
                return NotFound($"Sale with ID {saleId} not found.");

            if (sale.IsReturned)
                return BadRequest("This sale has already been returned.");

            sale.IsReturned = true;
            sale.ReturnReason = dto.ReturnReason ?? "No reason provided";

            sale.Product.Stock += sale.Quantity;
            var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == sale.ProductId);
            if (inventory != null)
            {
                inventory.Quantity += sale.Quantity;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Product '{sale.Product.Name}' has been returned successfully.",
                ReturnReason = sale.ReturnReason
            });
        }
    }
}
