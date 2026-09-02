using System.Net;
using System.Net.Http.Json;
using OrderService.DTOs;
using OrderService.Exceptions;

namespace OrderService.Clients;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductServiceClient> _logger;

    public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductStockDto?> GetProductAsync(Guid productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/products/{productId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductStockDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Product Service while fetching product {ProductId}", productId);
            throw new ProductServiceUnavailableException(ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timed out calling Product Service for product {ProductId}", productId);
            throw new ProductServiceUnavailableException("Request timed out.");
        }
    }

    // Calls Product Service's atomic reserve-stock endpoint. Product Service
    // performs the check-and-deduct as a single UPDATE, so this call is the
    // source of truth for whether stock was actually available.
    public async Task<StockReservationResult> ReserveStockAsync(Guid productId, int quantity)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/products/{productId}/reserve-stock",
                new { Quantity = quantity });

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return StockReservationResult.NotFound();
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var body = await response.Content.ReadFromJsonAsync<ProductServiceErrorBody>();
                return StockReservationResult.Insufficient(body?.AvailableStock ?? 0);
            }

            response.EnsureSuccessStatusCode();
            return StockReservationResult.Reserved();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Product Service while reserving stock for product {ProductId}", productId);
            throw new ProductServiceUnavailableException(ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timed out calling Product Service to reserve stock for product {ProductId}", productId);
            throw new ProductServiceUnavailableException("Request timed out.");
        }
    }

    // Best-effort compensating call. Failures here are logged but not thrown,
    // since the caller is already in an error-handling path.
    public async Task ReleaseStockAsync(Guid productId, int quantity)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/products/{productId}/release-stock",
                new { Quantity = quantity });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogCritical(
                    "Failed to release {Quantity} units for product {ProductId} after order save failure. Manual reconciliation may be required. Status: {Status}",
                    quantity, productId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex,
                "Exception releasing {Quantity} units for product {ProductId} after order save failure. Manual reconciliation may be required.",
                quantity, productId);
        }
    }

    // Loosely matches Product Service's 409 error payload; AvailableStock may
    // not always be present depending on how the error body is shaped.
    private class ProductServiceErrorBody
    {
        public int AvailableStock { get; set; }
        public string? Message { get; set; }
    }
}
