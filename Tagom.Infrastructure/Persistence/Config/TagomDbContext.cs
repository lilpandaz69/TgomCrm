using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;

namespace Tagom.Infrastructure.Persistence
{
    public class TagomDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Invoice)
                .WithMany(i => i.Items)
                .HasForeignKey(s => s.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet <Sale> Sales { get; set; } = null!;

        public TagomDbContext(DbContextOptions<TagomDbContext> options) : base(options)
        {
        }

    }

}
