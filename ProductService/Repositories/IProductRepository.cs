using ProductService.Entities;

namespace ProductService.Repositories;

public class StockReservationResult
{
    public bool Success { get; private set; }
    public bool ProductFound { get; private set; }
    public int AvailableStock { get; private set; }

    public static StockReservationResult Ok() => new() { Success = true, ProductFound = true };

    public static StockReservationResult NotFound() => new() { Success = false, ProductFound = false };

    public static StockReservationResult InsufficientStock(int available) =>
        new() { Success = false, ProductFound = true, AvailableStock = available };
}

public interface IProductRepository
{
    Task<Product> AddAsync(Product product);
    Task<Product?> GetByIdAsync(Guid productId);
    Task<(List<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task<bool> UpdateAsync(Product product);
    Task<bool> SoftDeleteAsync(Guid productId);
    Task<StockReservationResult> ReserveStockAsync(Guid productId, int quantity);
    Task ReleaseStockAsync(Guid productId, int quantity);
}
