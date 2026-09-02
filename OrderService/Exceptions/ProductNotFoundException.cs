namespace OrderService.Exceptions;

// Raised when the referenced product does not exist in the Product Service.
public class ProductNotFoundException : AppExceptionBase
{
    public ProductNotFoundException(Guid productId)
        : base($"Product with id '{productId}' was not found.", StatusCodes.Status404NotFound)
    {
    }
}
