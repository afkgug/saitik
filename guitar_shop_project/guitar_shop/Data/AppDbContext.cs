using Microsoft.EntityFrameworkCore;
using guitar_shop.Models;

namespace guitar_shop.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.IsConfirmed).HasDefaultValue(false);
            entity.Property(e => e.ConfirmationToken).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(256);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.TotalAmount).IsRequired();
            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.HasOne<User>().WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.ProductName).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Price).IsRequired();
            entity.HasOne<Order>().WithMany(o => o.Items).HasForeignKey(i => i.OrderId);
        });
    }
}
