using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // POST api/products
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var created = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
    }

    // GET api/products/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(product);
    }

    // GET api/products?page=1&pageSize=10
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _productService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }

    // PUT api/products/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var updated = await _productService.UpdateAsync(id, request);
        return Ok(updated);
    }

    // DELETE api/products/{id}  (soft delete -> IsActive = false)
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }

    // POST api/products/{id}/reserve-stock
    // Internal API called by Order Service to atomically validate + deduct stock.
    // Not intended for direct external/public consumption.
    [HttpPost("{id:guid}/reserve-stock")]
    [ProducesResponseType(typeof(ReserveStockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReserveStock(Guid id, [FromBody] ReserveStockRequest request)
    {
        var result = await _productService.ReserveStockAsync(id, request);
        return Ok(result);
    }

    // POST api/products/{id}/release-stock
    // Internal API: compensating action used by Order Service to roll back
    // a reservation if the order could not be persisted afterwards.
    [HttpPost("{id:guid}/release-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReleaseStock(Guid id, [FromBody] ReleaseStockRequest request)
    {
        await _productService.ReleaseStockAsync(id, request);
        return NoContent();
    }
}
