using EasyGames.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Decimal precision (SQL Server default is fine, but make it explicit)
            builder.Entity<Product>()
                   .Property(p => p.Price)
                   .HasColumnType("decimal(18,2)");
            builder.Entity<Order>()
                   .Property(o => o.Total)
                   .HasColumnType("decimal(18,2)");
            builder.Entity<OrderItem>()
                   .Property(oi => oi.UnitBuyPrice)
                   .HasColumnType("decimal(18,2)");
            builder.Entity<OrderItem>()
                  .Property(oi => oi.UnitPrice)
                  .HasColumnType("decimal(18,2)");



            // Simple relations (optional; EF will infer most)
            builder.Entity<Order>()
                   .HasMany(o => o.Items)
                   .WithOne(i => i.Order!)
                   .HasForeignKey(i => i.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
