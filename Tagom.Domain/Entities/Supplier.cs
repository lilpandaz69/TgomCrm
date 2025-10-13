namespace Tagom.Domain.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
    }

}
