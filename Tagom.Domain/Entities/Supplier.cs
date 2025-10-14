namespace Tagom.Domain.Entities
{
    public class Supplier 
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
    }

}
