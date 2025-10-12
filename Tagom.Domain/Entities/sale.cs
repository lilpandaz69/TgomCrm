using System;
using System.Collections.Generic;

namespace Tagom.Domain.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        // Each sale can have multiple products (items)
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }

    public class SaleItem
    {
        public int Id { get; set; }

        public int SaleId { get; set; }
        public Sale Sale { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
