using OrderService.Entities;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<Order?> GetByIdAsync(Guid orderId);
    Task<(List<Order> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
}
