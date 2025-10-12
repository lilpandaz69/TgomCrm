using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagom.Application.DTOs
{
    public class ProductDto
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public int Stock { get; set; }
        public int SupplierId { get; set; } // ✅ Use ID not Name
    }
}
