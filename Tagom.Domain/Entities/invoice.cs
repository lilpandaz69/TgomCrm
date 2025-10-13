namespace Tagom.Domain.Entities
{
    public class Invoice
    {
        public int Id { get; set; } // Order number
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
    }

    public class SaleItem
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Subtotal => Quantity * UnitPrice;
    }
}
