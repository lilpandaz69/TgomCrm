using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;

namespace Tagom.Infrastructure.Persistence
{
    public class TagomDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<SaleItem> SaleItems { get; set; } = null!;

        public TagomDbContext(DbContextOptions<TagomDbContext> options) : base(options)
        {
        }
    }

}
