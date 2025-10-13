namespace Tagom.Domain.Entities
{
    public class Invoice
    {
        public int Id { get; set; } 
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public ICollection<Sale> Items { get; set; } = new List<Sale>();

        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
    }
}


    
