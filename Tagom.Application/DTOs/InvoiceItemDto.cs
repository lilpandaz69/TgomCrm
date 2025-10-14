namespace Tagom.Application.DTOs
{
    public class InvoiceItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int OrderNumber { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? CustomerPhone { get; set; }
        public DateTime SaleDate { get; set; }

        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
        public decimal TotalAmount { get; set; }
    }

    public class CreateInvoiceDto
    {
        public int CustomerId { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
    }
}
