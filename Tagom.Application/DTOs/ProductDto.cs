using Microsoft.AspNetCore.Http;

namespace Tagom.Application.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public int Stock { get; set; }
        public int SupplierId { get; set; }

        // New property for file upload
        public IFormFile? ImageFile { get; set; }
        public decimal Orignailprice { get; set; }
    }
}
