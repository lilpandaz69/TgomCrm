using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;

namespace TgomCRM.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SupplierStock> SupplierStocks => Set<SupplierStock>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Supplier>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
        });

        b.Entity<Product>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.DefaultSellPrice).HasPrecision(18, 2);
        });

        b.Entity<SupplierStock>(e =>
        {
            e.HasKey(ss => new { ss.SupplierId, ss.ProductId });
            e.Property(ss => ss.QtyOnHand).IsRequired();
            e.Property(ss => ss.LastUnitCost).HasPrecision(18, 2);
            e.Property(ss => ss.RowVersion).IsRowVersion();
            e.HasOne(ss => ss.Supplier).WithMany(s => s.Stocks).HasForeignKey(ss => ss.SupplierId);
            e.HasOne(ss => ss.Product).WithMany(p => p.Stocks).HasForeignKey(ss => ss.ProductId);
        });

        b.Entity<Sale>(e =>
        {
            e.Property(s => s.InvoiceNumber).HasMaxLength(32).IsRequired();
            e.Property(s => s.Subtotal).HasPrecision(18, 2);
            e.Property(s => s.Discount).HasPrecision(18, 2);
            e.Property(s => s.Tax).HasPrecision(18, 2);
            e.Property(s => s.Total).HasPrecision(18, 2);
            e.HasIndex(s => s.InvoiceNumber).IsUnique();
        });

        b.Entity<SaleItem>(e =>
        {
            e.Property(si => si.UnitPrice).HasPrecision(18, 2);
            e.HasOne(si => si.Sale).WithMany(s => s.Items).HasForeignKey(si => si.SaleId);
            e.HasOne(si => si.Product).WithMany().HasForeignKey(si => si.ProductId);
        });

        // Read-only view: total inventory per product (sum over suppliers)
        b.Entity<ProductInventoryView>().HasNoKey().ToView("vw_ProductInventory");
    }

    // ===== Inventory operations (no Purchase docs) =====
    public async Task AdjustStockAsync(int supplierId, int productId, int deltaQty, decimal? unitCost = null, CancellationToken ct = default)
    {
        // deltaQty > 0: add stock, deltaQty < 0: remove stock
        var ss = await SupplierStocks.FindAsync(new object?[] { supplierId, productId }, ct);
        if (ss is null)
        {
            if (deltaQty < 0)
                throw new InvalidOperationException("Cannot decrement stock that doesn't exist");
            ss = new SupplierStock { SupplierId = supplierId, ProductId = productId, QtyOnHand = 0 };
            SupplierStocks.Add(ss);
        }
        if (unitCost.HasValue) ss.LastUnitCost = unitCost.Value;
        var newQty = ss.QtyOnHand + deltaQty;
        if (newQty < 0) throw new InvalidOperationException("Not enough stock");
        ss.QtyOnHand = newQty;
        await SaveChangesAsync(ct);
    }

    public async Task<Sale> CreateSaleAsync(Sale sale, CancellationToken ct = default)
    {
        await Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            using var tx = await Database.BeginTransactionAsync(ct);

            Sales.Add(sale);

            foreach (var it in sale.Items)
            {
                var ss = await SupplierStocks.FindAsync(new object?[] { it.SupplierId, it.ProductId }, ct)
                         ?? throw new InvalidOperationException($"Stock not found for supplier {it.SupplierId} product {it.ProductId}");
                if (ss.QtyOnHand < it.Quantity)
                    throw new InvalidOperationException($"Not enough stock for product {it.ProductId} from supplier {it.SupplierId}");
                ss.QtyOnHand -= it.Quantity;
            }

            await SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
        return sale;
    }
}

public class ProductInventoryView
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int TotalQtyOnHand { get; set; }
}