using Microsoft.EntityFrameworkCore;
using TestCoreApi.Models;

namespace TestCoreApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<User>    Users    => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Category).HasMaxLength(100);
            });

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop",      Description = "High-performance laptop", Price = 999.99m,  Stock = 10, Category = "Electronics", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Product { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 29.99m, Stock = 50, Category = "Electronics", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Product { Id = 3, Name = "Desk Chair",  Description = "Comfortable office chair",  Price = 249.99m, Stock = 15, Category = "Furniture",   CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
            });
        }
    }
}
