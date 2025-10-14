using System.ComponentModel.DataAnnotations;

namespace Tagom.Domain.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        [EmailAddress]
        public string ? Email { get; set; }
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        public ICollection<Sale> sales { get; set; }
    }
}
