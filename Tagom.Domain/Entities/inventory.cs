namespace Tagom.Domain.Entities
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }

        public void AddStock(int amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
            Quantity += amount;
        }

        public void RemoveStock(int amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
            if (amount > Quantity) throw new InvalidOperationException("Not enough stock available.");
            Quantity -= amount;
        }
    }

}
