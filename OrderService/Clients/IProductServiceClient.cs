using OrderService.DTOs;

namespace OrderService.Clients;

public enum StockReservationOutcome
{
    Reserved,
    ProductNotFound,
    InsufficientStock
}

public class StockReservationResult
{
    public StockReservationOutcome Outcome { get; init; }
    public int AvailableStock { get; init; }

    public static StockReservationResult Reserved() => new() { Outcome = StockReservationOutcome.Reserved };
    public static StockReservationResult NotFound() => new() { Outcome = StockReservationOutcome.ProductNotFound };
    public static StockReservationResult Insufficient(int available) =>
        new() { Outcome = StockReservationOutcome.InsufficientStock, AvailableStock = available };
}

// Abstraction over the Product Service HTTP API. This is the ONLY way
// Order Service is allowed to interact with product data - it must never
// open a connection to product_db directly.
public interface IProductServiceClient
{
    Task<ProductStockDto?> GetProductAsync(Guid productId);
    Task<StockReservationResult> ReserveStockAsync(Guid productId, int quantity);
    Task ReleaseStockAsync(Guid productId, int quantity);
}
