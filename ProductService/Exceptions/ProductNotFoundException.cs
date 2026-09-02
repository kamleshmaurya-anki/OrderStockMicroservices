namespace ProductService.Exceptions;

public class ProductNotFoundException : AppExceptionBase
{
    public ProductNotFoundException(Guid productId)
        : base($"Product with id '{productId}' was not found.", StatusCodes.Status404NotFound)
    {
    }
}
