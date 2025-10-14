namespace Tagom.Domain.Entities
{
    public class SupplierInvoice
    {
        public int SupplierInvoiceId { get; set; }

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public decimal SubTotal { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<SupplierInvoiceItem> Items { get; set; } = new List<SupplierInvoiceItem>();
    }
}
