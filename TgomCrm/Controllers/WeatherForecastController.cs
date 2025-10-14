using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
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

    // ==============================
    // ✅ PRODUCTS CONTROLLER
    // ==============================
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
                    product.Category,
                    product.Stock,
                    product.ImageUrl,
                    Supplier = new { supplier.SupplierId, supplier.Name }
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

    // ==============================
    // ✅ SALES CONTROLLER (with Return Feature)
    // ==============================
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

    // ==============================
    // ✅ CUSTOMERS CONTROLLER
    // ==============================
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly TagomDbContext _db;
        public CustomersController(TagomDbContext db) => _db = db;

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

    // ==============================
    // ✅ INVOICES CONTROLLER
    // ==============================
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
    }
}
