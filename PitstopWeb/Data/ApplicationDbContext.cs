using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Models;

namespace PitstopWeb.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCompatibility> ProductCompatibilities => Set<ProductCompatibility>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductCompatibility>(entity =>
        {
            entity.HasOne(pc => pc.Product)
                .WithMany(p => p.Compatibility)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Total).HasColumnType("decimal(10,2)");
            entity.HasIndex(o => o.StripeSessionId).IsUnique();
            entity.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.PriceAtPurchase).HasColumnType("decimal(10,2)");
            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
