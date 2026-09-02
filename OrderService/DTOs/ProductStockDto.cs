namespace OrderService.DTOs;

// Shape returned by Product Service's GET /api/products/{id} endpoint.
// Order Service only ever sees this DTO over HTTP - it never queries product_db.
public class ProductStockDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public bool IsActive { get; set; }
}
