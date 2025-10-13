namespace Tagom.Domain.Entities
{
    public class Inventory
    {
        public int Id { get; set; }

        // Foreign key to Product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Quantity in stock
        public int Quantity { get; set; }

        // Helper methods
        public void AddStock(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");
            Quantity += amount;
        }

        public void RemoveStock(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");
            if (amount > Quantity)
                throw new InvalidOperationException("Not enough stock available.");
            Quantity -= amount;
        }
    }
}
