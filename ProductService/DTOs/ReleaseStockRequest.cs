using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

// Compensating action: called by Order Service if it reserved stock
// but then failed to persist the order, so the units are returned.
public class ReleaseStockRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}
