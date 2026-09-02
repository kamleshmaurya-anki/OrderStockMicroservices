namespace OrderService.Exceptions;

// Raised when Order Service cannot reach Product Service at all
// (network failure, timeout, non-JSON response, etc.).
public class ProductServiceUnavailableException : AppExceptionBase
{
    public ProductServiceUnavailableException(string reason)
        : base($"Product Service is currently unavailable: {reason}", StatusCodes.Status503ServiceUnavailable)
    {
    }
}
