using Microsoft.EntityFrameworkCore;
using Tagom.Domain.Entities;

namespace Tagom.Infrastructure.Persistence
{
    public class TagomDbContext : DbContext
    {
        public TagomDbContext(DbContextOptions<TagomDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(e =>
            {
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.Email).HasMaxLength(200);
                e.Property(x => x.Phone).HasMaxLength(50);
                e.Property(x => x.Address).HasMaxLength(300);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
