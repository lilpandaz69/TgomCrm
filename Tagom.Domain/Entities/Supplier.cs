namespace Tagom.Domain.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

}
