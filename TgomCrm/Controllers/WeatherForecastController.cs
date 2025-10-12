using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;
using TgomCRM.Infrastructure.Persistence;

namespace TgomCrm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly AppDbContext _db;
    public SuppliersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Suppliers.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Supplier s)
    {
        if (string.IsNullOrWhiteSpace(s.Name)) return BadRequest("Name is required");
        _db.Suppliers.Add(s);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = s.Id }, s);
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Products.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product p)
    {
        if (string.IsNullOrWhiteSpace(p.Name)) return BadRequest("Name is required");
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = p.Id }, p);
    }
}

// Stock adjustments (receive or correct)
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _db;
    public InventoryController(AppDbContext db) => _db = db;

    public record AdjustDto(int SupplierId, int ProductId, int DeltaQty, decimal? UnitCost);

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustDto dto)
    {
        try
        {
            await _db.AdjustStockAsync(dto.SupplierId, dto.ProductId, dto.DeltaQty, dto.UnitCost);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _db;
    public SalesController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Sale sale)
    {
        if (!sale.Items.Any()) return BadRequest("Sale must have at least one item");

        sale.Subtotal = sale.Items.Sum(i => i.UnitPrice * i.Quantity);
        sale.Total = sale.Subtotal - sale.Discount + sale.Tax;
        sale.InvoiceNumber = $"INV-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..5].ToUpper()}";

        try
        {
            await _db.CreateSaleAsync(sale);
            return Ok(new { sale.Id, sale.InvoiceNumber });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
