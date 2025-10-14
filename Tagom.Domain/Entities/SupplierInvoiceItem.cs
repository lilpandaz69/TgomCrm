namespace Tagom.Domain.Entities
{
    public class SupplierInvoiceItem
    {
        public int SupplierInvoiceItemId { get; set; }

        public int SupplierInvoiceId { get; set; }
        public SupplierInvoice SupplierInvoice { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }


        public decimal LineTotal => Quantity * UnitCost;
    }
}
