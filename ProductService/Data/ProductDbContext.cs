using Microsoft.EntityFrameworkCore;
using ProductService.Entities;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(p => p.ProductId);

            entity.Property(p => p.ProductId)
                  .HasColumnName("product_id")
                  .HasDefaultValueSql("NEWID()");

            entity.Property(p => p.ProductName)
                  .HasColumnName("product_name")
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(p => p.Price)
                  .HasColumnName("price")
                  .HasColumnType("decimal(10,2)");

            entity.Property(p => p.StockQty)
                  .HasColumnName("stock_qty")
                  .IsRequired();

            entity.Property(p => p.IsActive)
                  .HasColumnName("is_active")
                  .HasDefaultValue(true);

            entity.Property(p => p.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(p => p.UpdatedAt)
                  .HasColumnName("updated_at");

            entity.ToTable(t => t.HasCheckConstraint("chk_price_nonnegative", "[price] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("chk_stock_nonnegative", "[stock_qty] >= 0"));

            entity.HasIndex(p => p.ProductName).HasDatabaseName("idx_product_name");
        });
    }
}
