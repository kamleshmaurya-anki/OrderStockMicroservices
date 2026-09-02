using ProductService.DTOs;

namespace ProductService.Services;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> GetByIdAsync(Guid productId);
    Task<PagedResult<ProductResponse>> GetPagedAsync(int page, int pageSize);
    Task<ProductResponse> UpdateAsync(Guid productId, UpdateProductRequest request);
    Task DeleteAsync(Guid productId);
    Task<ReserveStockResponse> ReserveStockAsync(Guid productId, ReserveStockRequest request);
    Task ReleaseStockAsync(Guid productId, ReleaseStockRequest request);
}
