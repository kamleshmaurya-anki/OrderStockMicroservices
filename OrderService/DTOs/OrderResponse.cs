namespace OrderService.DTOs;

public class OrderResponse
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
