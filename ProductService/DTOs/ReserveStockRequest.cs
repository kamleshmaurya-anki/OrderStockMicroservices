using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

// Called internally by Order Service to atomically validate + deduct stock.
public class ReserveStockRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}
