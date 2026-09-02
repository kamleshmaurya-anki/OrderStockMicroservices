namespace ProductService.DTOs;

public class ReserveStockResponse
{
    public bool Success { get; set; }
    public Guid ProductId { get; set; }
    public int RemainingStock { get; set; }
    public string Message { get; set; } = string.Empty;
}
