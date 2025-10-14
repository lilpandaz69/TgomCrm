using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagom.Domain.Entities
{
    public class Sale
    {

        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public string CustomerPhone { get; set; }

        public int? InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public bool IsReturned { get; set; } = false;
        public string? ReturnReason { get; set; } 

        public decimal Subtotal => Quantity * UnitPrice;


  


    }
}

