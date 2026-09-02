namespace OrderService.Entities;

public static class OrderStatus
{
    public const string Created = "CREATED";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";
}

public class Order
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string OrderStatus { get; set; } = Entities.OrderStatus.Created;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
