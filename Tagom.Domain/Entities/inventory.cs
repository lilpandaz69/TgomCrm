namespace Tagom.Domain.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Tracks the available quantity of the product
        public int Quantity { get; set; }

        // Helper methods for stock management
        public void AddStock(int amount)
        {
            Quantity += amount;
        }

        public void RemoveStock(int amount)
        {
            if (amount > Quantity)
                throw new InvalidOperationException("Not enough stock available.");

            Quantity -= amount;
        }
    }
}
