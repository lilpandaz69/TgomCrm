using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tagom.Domain.Entities;

namespace Tagom.Application.DTOs
{
    public class SaleDto
    {
        public string CustomerPhone { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
}
