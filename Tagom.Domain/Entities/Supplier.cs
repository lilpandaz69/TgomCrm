namespace Tagom.Domain.Entities
{
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
