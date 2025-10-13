using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;
using Tagom.Infrastructure.Persistence;
using Tagom.Application.DTOs;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _db.Suppliers
                .AsNoTracking()
                .Select(s => new { s.Id, s.Name, s.Phone })
                .ToListAsync();
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _db.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new { s.Id, s.Name, s.Phone })
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

            return CreatedAtAction(nameof(GetById), new { id = s.Id }, new { s.Id, s.Name, s.Phone });
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
                    Supplier = new { p.Supplier.Id, p.Supplier.Name }
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
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Category,
                    p.Stock,
                    Supplier = new { p.Supplier.Id, p.Supplier.Name }
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            var supplier = await _db.Suppliers.FindAsync(dto.SupplierId);
            if (supplier == null)
                return BadRequest($"Supplier with ID {dto.SupplierId} not found.");

            var existingProduct = await _db.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Name.ToLower() == dto.Name.ToLower() && p.SupplierId == dto.SupplierId);

            if (existingProduct != null)
            {
                existingProduct.Stock += dto.Stock;
                existingProduct.Price = dto.Price;
                existingProduct.Category = dto.Category;

                if (existingProduct.Inventory == null)
                    existingProduct.Inventory = new Inventory { ProductId = existingProduct.Id, Quantity = existingProduct.Stock };
                else
                    existingProduct.Inventory.Quantity += dto.Stock;

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
                        Supplier = new { supplier.Id, supplier.Name }
                    }
                });
            }

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Category = dto.Category,
                Stock = dto.Stock,
                SupplierId = dto.SupplierId,
                Inventory = new Inventory { Quantity = dto.Stock }
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new
            {
                message = "New product created successfully with inventory record.",
                product = new
                {
                    product.Id,
                    product.Name,
                    product.Price,
                    product.Category,
                    product.Stock,
                    Supplier = new { supplier.Id, supplier.Name }
                }
            });
        }
    }

    // ==============================
    // ✅ INVENTORY CONTROLLER
    // ==============================
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
                    Products = categoryGroup.GroupBy(x => new { x.Product.Id, x.Product.Name })
                        .Select(productGroup => new
                        {
                            ProductId = productGroup.Key.Id,
                            ProductName = productGroup.Key.Name,
                            TotalStock = productGroup.Sum(x => x.Quantity),
                            Suppliers = productGroup.Select(x => new
                            {
                                SupplierId = x.Product.Supplier.Id,
                                SupplierName = x.Product.Supplier.Name,
                                StockFromSupplier = x.Quantity
                            }).ToList()
                        }).ToList()
                })
                .ToListAsync();

            return Ok(inventoryData);
        }

        [HttpPost("increase/{productId}")]
        public async Task<IActionResult> IncreaseStock(int productId, [FromQuery] int amount)
        {
            if (amount <= 0) return BadRequest("Increase amount must be greater than 0.");

            var product = await _db.Products.FindAsync(productId);
            if (product == null) return NotFound($"Product with ID {productId} not found.");

            var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory == null)
            {
                inventory = new Inventory { ProductId = productId, Quantity = amount };
                _db.Inventories.Add(inventory);
            }
            else
            {
                inventory.AddStock(amount);
            }

            product.Stock += amount;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Increased stock for '{product.Name}' by {amount}.",
                inventory = new { product.Id, product.Name, inventory.Quantity }
            });
        }

        [HttpPost("decrease/{productId}")]
        public async Task<IActionResult> DecreaseStock(int productId, [FromQuery] int amount)
        {
            if (amount <= 0) return BadRequest("Decrease amount must be greater than 0.");

            var product = await _db.Products.FindAsync(productId);
            if (product == null) return NotFound($"Product with ID {productId} not found.");

            var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory == null) return NotFound("Inventory record not found for this product.");

            try
            {
                inventory.RemoveStock(amount);
                product.Stock -= amount;
                if (product.Stock < 0) product.Stock = 0;
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Decreased stock for '{product.Name}' by {amount}.",
                inventory = new { product.Id, product.Name, inventory.Quantity }
            });
        }
    }

    // ==============================
    // ✅ INVOICE CONTROLLER
    // ==============================
    //[ApiController]
    //[Route("api/[controller]")]
    //public class InvoiceController : ControllerBase
    //{
    //    private readonly TagomDbContext _db;
    //    public InvoiceController(TagomDbContext db) => _db = db;

    //    // GET invoice by ID
    //    [HttpGet("{id}")]
    //    public async Task<IActionResult> GetInvoice(int id)
    //    {
    //        var invoice = await _db.Invoices
    //            .Include(i => i.Customer)
    //            .Include(i => i.Items)
    //                .ThenInclude(si => si.Product)
    //            .AsNoTracking()
    //            .FirstOrDefaultAsync(i => i.Id == id);

    //        if (invoice == null) return NotFound($"Invoice {id} not found.");

    //        var dto = new InvoiceDto
    //        {
    //            OrderNumber = invoice.Id,
    //            CustomerName = invoice.Customer.Name,
    //            CustomerPhone = invoice.Customer.Phone,
    //            SaleDate = invoice.SaleDate,
    //            Items = invoice.Items.Select(i => new InvoiceItemDto
    //            {
    //                ProductId = i.ProductId,
    //                ProductName = i.Product.Name,
    //                Quantity = i.Quantity,
    //                UnitPrice = i.UnitPrice
    //            }).ToList(),
    //            TotalAmount = invoice.TotalAmount
    //        };

    //        return Ok(dto);
    //    }

    //    // POST create invoice (decrease inventory)
    //    [HttpPost]
    //    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
    //    {
    //        var customer = await _db.Customers.FindAsync(dto.CustomerId);
    //        if (customer == null) return BadRequest("Customer not found.");

    //        var invoice = new Invoice { CustomerId = dto.CustomerId };

    //        foreach (var item in dto.Items)
    //        {
    //            var product = await _db.Products
    //                .Include(p => p.Inventory)
    //                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

    //            if (product == null) return BadRequest($"Product {item.ProductId} not found.");
    //            if (product.Inventory == null) return BadRequest($"Inventory for {product.Name} not found.");
    //            if (product.Inventory.Quantity < item.Quantity) return BadRequest($"Not enough stock for {product.Name}.");

    //            product.Inventory.RemoveStock(item.Quantity);
    //            product.Stock -= item.Quantity;

    //            invoice.Items.Add(new SaleItem
    //            {
    //                ProductId = item.ProductId,
    //                Quantity = item.Quantity,
    //                UnitPrice = item.UnitPrice
    //            });
    //        }

    //        _db.Invoices.Add(invoice);
    //        await _db.SaveChangesAsync();

    //        return Ok(new { message = "Invoice created and stock updated.", invoiceId = invoice.Id });
    //    }

    //    // POST retrieve items (increase inventory)
    //    [HttpPost("retrieve/{invoiceId}")]
    //    public async Task<IActionResult> RetrieveItems(int invoiceId)
    //    {
    //        var invoice = await _db.Invoices
    //            .Include(i => i.Items)
    //                .ThenInclude(si => si.Product)
    //                    .ThenInclude(p => p.Inventory)
    //            .FirstOrDefaultAsync(i => i.Id == invoiceId);

    //        if (invoice == null) return NotFound($"Invoice {invoiceId} not found.");

    //        foreach (var item in invoice.Items)
    //        {
    //            if (item.Product.Inventory != null)
    //                item.Product.Inventory.AddStock(item.Quantity);

    //            item.Product.Stock += item.Quantity;
    //        }

    //        await _db.SaveChangesAsync();
    //        return Ok(new { message = $"Inventory restored for Invoice {invoiceId}." });
    //    }
    //}
}
