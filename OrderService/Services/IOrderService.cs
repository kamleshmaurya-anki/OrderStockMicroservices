using OrderService.DTOs;

namespace OrderService.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse> GetByIdAsync(Guid orderId);
    Task<PagedResult<OrderResponse>> GetPagedAsync(int page, int pageSize);
}
