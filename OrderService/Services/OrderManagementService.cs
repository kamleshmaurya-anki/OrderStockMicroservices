using OrderService.Clients;
using OrderService.DTOs;
using OrderService.Entities;
using OrderService.Exceptions;
using OrderService.Repositories;

namespace OrderService.Services;

public class OrderManagementService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ILogger<OrderManagementService> _logger;

    public OrderManagementService(
        IOrderRepository orderRepository,
        IProductServiceClient productServiceClient,
        ILogger<OrderManagementService> logger)
    {
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        // Order Service never touches product_db directly - stock is validated
        // and deducted atomically by Product Service via this single call.
        var reservation = await _productServiceClient.ReserveStockAsync(request.ProductId, request.Quantity);

        switch (reservation.Outcome)
        {
            case StockReservationOutcome.ProductNotFound:
                _logger.LogWarning("Order rejected: product {ProductId} not found", request.ProductId);
                throw new ProductNotFoundException(request.ProductId);

            case StockReservationOutcome.InsufficientStock:
                _logger.LogWarning(
                    "Order rejected: insufficient stock for product {ProductId}. Requested {Requested}, available {Available}",
                    request.ProductId, request.Quantity, reservation.AvailableStock);
                throw new InsufficientStockException(request.ProductId, request.Quantity, reservation.AvailableStock);

            case StockReservationOutcome.Reserved:
            default:
                break;
        }

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            OrderStatus = Entities.OrderStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            var created = await _orderRepository.AddAsync(order);
            _logger.LogInformation(
                "Created order {OrderId} for product {ProductId}, quantity {Quantity}",
                created.OrderId, created.ProductId, created.Quantity);

            return Map(created);
        }
        catch (Exception ex)
        {
            // Stock was already deducted on Product Service. Since order_db
            // failed to persist, compensate by releasing the reserved units
            // back so the two services don't drift out of sync.
            _logger.LogError(ex,
                "Failed to persist order for product {ProductId} after stock was reserved. Releasing reserved stock.",
                request.ProductId);
            await _productServiceClient.ReleaseStockAsync(request.ProductId, request.Quantity);
            throw;
        }
    }

    public async Task<OrderResponse> GetByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new OrderNotFoundException(orderId);
        }

        return Map(order);
    }

    public async Task<PagedResult<OrderResponse>> GetPagedAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var (items, totalCount) = await _orderRepository.GetPagedAsync(page, pageSize);

        return new PagedResult<OrderResponse>
        {
            Items = items.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static OrderResponse Map(Order order) => new()
    {
        OrderId = order.OrderId,
        ProductId = order.ProductId,
        Quantity = order.Quantity,
        OrderStatus = order.OrderStatus,
        CreatedAt = order.CreatedAt
    };
}
