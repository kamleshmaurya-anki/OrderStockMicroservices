using Microsoft.EntityFrameworkCore;
using OrderService.Entities;

namespace OrderService.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(o => o.OrderId);

            entity.Property(o => o.OrderId)
                  .HasColumnName("order_id")
                  .HasDefaultValueSql("NEWID()");

            entity.Property(o => o.ProductId)
                  .HasColumnName("product_id")
                  .IsRequired();

            entity.Property(o => o.Quantity)
                  .HasColumnName("quantity")
                  .IsRequired();

            entity.Property(o => o.OrderStatus)
                  .HasColumnName("order_status")
                  .HasMaxLength(30)
                  .HasDefaultValue(Entities.OrderStatus.Created)
                  .IsRequired();

            entity.Property(o => o.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.ToTable(t => t.HasCheckConstraint("chk_order_quantity_positive", "[quantity] > 0"));
            entity.ToTable(t => t.HasCheckConstraint(
                "chk_order_status",
                "[order_status] IN ('CREATED','PAID','CANCELLED')"));

            entity.HasIndex(o => o.ProductId).HasDatabaseName("idx_orders_product_id");
        });
    }
}
