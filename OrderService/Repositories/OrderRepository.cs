using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Entities;

namespace OrderService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public Task<Order?> GetByIdAsync(Guid orderId)
    {
        return _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Orders.AsNoTracking().OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
