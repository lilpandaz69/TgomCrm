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
    public class InvoicesController : ControllerBase
    {
        private readonly TagomDbContext _db;
        public InvoicesController(TagomDbContext db) => _db = db;

        [HttpGet("by-sale/{saleId}")]
        public async Task<IActionResult> GetBySaleId(int saleId)
        {
            var sale = await _db.Sales
                .Include(s => s.Customer)
                .Include(s => s.Product)
                .ThenInclude(p => p.Supplier)
                .Include(s => s.Invoice)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SaleId == saleId);

            if (sale == null)
                return NotFound($"❌ No sale found with ID {saleId}.");

            return Ok(new
            {
                SaleId = sale.SaleId,
                SaleDate = sale.SaleDate,
                Customer = new
                {
                    sale.Customer.CustomerId,
                    sale.Customer.Name,
                    sale.Customer.Phone,
                    sale.Customer.Email
                },
                Product = new
                {
                    sale.Product.ProductId,
                    sale.Product.Name,
                    Supplier = sale.Product.Supplier != null
                        ? new { sale.Product.Supplier.SupplierId, sale.Product.Supplier.Name }
                        : null
                },
                sale.Quantity,
                sale.UnitPrice,
                sale.TotalPrice,
                Invoice = sale.Invoice != null
                    ? new { sale.Invoice.InvoiceId, sale.Invoice.SaleDate }
                    : null
            });
        }


        [HttpGet("overview/{supplierId}/{productId}")]
        public async Task<IActionResult> GetSupplierProductOverview(int supplierId, int productId)
        {
            var supplier = await _db.Suppliers
                .AsNoTracking()
                .Where(s => s.SupplierId == supplierId)
                .Select(s => new { s.SupplierId, s.Name, s.Phone })
                .FirstOrDefaultAsync();

            if (supplier == null)
                return NotFound($"Supplier with ID {supplierId} not found.");

            var product = await _db.Products
                .AsNoTracking()
                .Where(p => p.SupplierId == supplierId && p.ProductId == productId)
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Orignailprice,
                    p.Category,
                    p.Stock
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound($"Product with ID {productId} not found for this supplier.");

            var inventory = await _db.Inventories
                .Include(i => i.Product)
                .AsNoTracking()
                .Where(i => i.Product.SupplierId == supplierId && i.ProductId == productId)
                .Select(i => new
                {
                    i.ProductId,
                    ProductName = i.Product.Name,
                    QuantityInInventory = i.Quantity,
                    Price = i.Product.Orignailprice,
                    Category = i.Product.Category,
                    TotalPrice = i.Quantity * i.Product.Orignailprice
                })
                .FirstOrDefaultAsync();

            // 🕓 توقيت مصر
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            // ==== هنا نحفظ فاتورة بسيطة (نخزن SupplierId + InvoiceDate + مبالغ صفرية)
            var invoice = new SupplierInvoice
            {
                SupplierId = supplierId,
                InvoiceDate = egyptTime,
                SubTotal = 0m,
                TotalAmount = 0m
            };

            _db.SupplierInvoices.Add(invoice);
            await _db.SaveChangesAsync(); // بعد الحفظ الـ invoice.SupplierInvoiceId هيتعبى تلقائياً

            // ==== نرجع الـ response مع الـ SupplierInvoiceId اللي اتخزن
            return Ok(new
            {
                Supplier = supplier,
                Product = product,
                Inventory = inventory,
                InvoiceDate = egyptTime,
                SupplierInvoiceId = invoice.SupplierInvoiceId
            });
        }

    }
}
