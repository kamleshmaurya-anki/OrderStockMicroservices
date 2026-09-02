using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Exceptions;
using ProductService.Repositories;

namespace ProductService.Services;

public class ProductManagementService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductManagementService> _logger;

    public ProductManagementService(IProductRepository repository, ILogger<ProductManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            ProductName = request.ProductName,
            Price = request.Price,
            StockQty = request.StockQty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(product);
        _logger.LogInformation("Created product {ProductId} ({ProductName})", created.ProductId, created.ProductName);

        return Map(created);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid productId)
    {
        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new ProductNotFoundException(productId);
        }

        return Map(product);
    }

    public async Task<PagedResult<ProductResponse>> GetPagedAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize);

        return new PagedResult<ProductResponse>
        {
            Items = items.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductResponse> UpdateAsync(Guid productId, UpdateProductRequest request)
    {
        var product = new Product
        {
            ProductId = productId,
            ProductName = request.ProductName,
            Price = request.Price,
            StockQty = request.StockQty,
            IsActive = request.IsActive
        };

        var updated = await _repository.UpdateAsync(product);
        if (!updated)
        {
            throw new ProductNotFoundException(productId);
        }

        _logger.LogInformation("Updated product {ProductId}", productId);
        return await GetByIdAsync(productId);
    }

    public async Task DeleteAsync(Guid productId)
    {
        var deleted = await _repository.SoftDeleteAsync(productId);
        if (!deleted)
        {
            throw new ProductNotFoundException(productId);
        }

        _logger.LogInformation("Soft-deleted product {ProductId}", productId);
    }

    public async Task<ReserveStockResponse> ReserveStockAsync(Guid productId, ReserveStockRequest request)
    {
        var result = await _repository.ReserveStockAsync(productId, request.Quantity);

        if (!result.ProductFound)
        {
            throw new ProductNotFoundException(productId);
        }

        if (!result.Success)
        {
            _logger.LogWarning(
                "Stock reservation failed for product {ProductId}. Requested: {Requested}, Available: {Available}",
                productId, request.Quantity, result.AvailableStock);
            throw new InsufficientStockException(productId, request.Quantity, result.AvailableStock);
        }

        _logger.LogInformation("Reserved {Quantity} units of product {ProductId}", request.Quantity, productId);

        return new ReserveStockResponse
        {
            Success = true,
            ProductId = productId,
            Message = "Stock reserved successfully."
        };
    }

    public async Task ReleaseStockAsync(Guid productId, ReleaseStockRequest request)
    {
        await _repository.ReleaseStockAsync(productId, request.Quantity);
        _logger.LogInformation("Released {Quantity} units back to product {ProductId}", request.Quantity, productId);
    }

    private static ProductResponse Map(Product product) => new()
    {
        ProductId = product.ProductId,
        ProductName = product.ProductName,
        Price = product.Price,
        StockQty = product.StockQty,
        IsActive = product.IsActive,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}
