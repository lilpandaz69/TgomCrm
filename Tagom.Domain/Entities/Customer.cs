namespace Tagom.Domain.Entities;

public enum PaymentMethod { Cash = 1, Card = 2, BankTransfer = 3, MobileWallet = 4 }
public enum SaleStatus { Draft = 0, Confirmed = 1, Cancelled = 2 }

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    public ICollection<SupplierStock> Stocks { get; set; } = new List<SupplierStock>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public decimal DefaultSellPrice { get; set; }

    public ICollection<SupplierStock> Stocks { get; set; } = new List<SupplierStock>();
}

// === The single source of truth for inventory ===
// One row per (Supplier, Product) with a running QtyOnHand. No Purchase docs.
public class SupplierStock
{
    public int SupplierId { get; set; }
    public int ProductId { get; set; }

    public int QtyOnHand { get; set; } // total stock from this supplier for this product
    public decimal? LastUnitCost { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Supplier Supplier { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public class Sale
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public int? CustomerId { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public SaleStatus Status { get; set; } = SaleStatus.Confirmed;

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }

    // Which supplier’s stock did we consume?
    public int SupplierId { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
