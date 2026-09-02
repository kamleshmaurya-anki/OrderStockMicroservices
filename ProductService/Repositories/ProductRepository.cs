using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Entities;

namespace ProductService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public Task<Product?> GetByIdAsync(Guid productId)
    {
        return _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Products.AsNoTracking().OrderBy(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        var existing = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
        if (existing == null)
        {
            return false;
        }

        existing.ProductName = product.ProductName;
        existing.Price = product.Price;
        existing.StockQty = product.StockQty;
        existing.IsActive = product.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteAsync(Guid productId)
    {
        var existing = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (existing == null)
        {
            return false;
        }

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // Atomically checks and deducts stock in a single UPDATE statement so
    // concurrent order requests can never both succeed against the same units.
    public async Task<StockReservationResult> ReserveStockAsync(Guid productId, int quantity)
    {
        var product = await _context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null || !product.IsActive)
        {
            return StockReservationResult.NotFound();
        }

        var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE products
            SET stock_qty = stock_qty - {quantity}, updated_at = GETUTCDATE()
            WHERE product_id = {productId} AND stock_qty >= {quantity} AND is_active = 1");

        if (rowsAffected == 0)
        {
            var latest = await _context.Products.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            return StockReservationResult.InsufficientStock(latest?.StockQty ?? 0);
        }

        return StockReservationResult.Ok();
    }

    // Compensating action for a previously successful reservation that could
    // not be committed on the caller's side (e.g. order row failed to save).
    public async Task ReleaseStockAsync(Guid productId, int quantity)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE products
            SET stock_qty = stock_qty + {quantity}, updated_at = GETUTCDATE()
            WHERE product_id = {productId}");
    }
}
