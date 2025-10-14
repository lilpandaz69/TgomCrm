namespace Tagom.Domain.Entities
{
    public class Product 
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public int Stock { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;
        public Inventory? Inventory { get; set; }
        public string? ImageUrl { get; set; }

    }

}
