namespace Tagom.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }
}
