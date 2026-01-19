using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Interfaces;
using OrderManagement.Communication.Dtos.Product;
using OrderManagement.Communication.Responses;

namespace OrderManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto requestDto)
    {
        var result = await _productService.Create(requestDto);
        
        if(!result.Success)
            return BadRequest(result.ErrorMessage);

        return CreatedAtAction(nameof(GetProducById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] UpdateProductDto requestDto)
    {
        var result = await _productService.Update(id, requestDto);
        
        if(!result.Success)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProducById([FromRoute] Guid id)
    {
        var result = await _productService.GetProductById(id);
        
        if(!result.Success)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProducts()
    {
        var result = await _productService.GetAll();
        
        if(!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
    {
        var result = await _productService.Delete(id);
        
        if(!result.Success)
            return NotFound(result.ErrorMessage);

        return NoContent();
    }
}
