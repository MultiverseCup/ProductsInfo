using Microsoft.AspNetCore.Mvc;
using ProductsInfo.Application.Abstractions;
using ProductsInfo.Application.Contracts;
using ProductsInfo.Presentation.Models;

namespace ProductsInfo.Presentation.Controllers
{
    [ApiController]
    [Route("api/products")]
    public sealed class ProductsController(
    IProductService productService
) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(
            typeof(CreateProductResponse),
            StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateProductResponse>> Create(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateProductCommand(
                request.Name,
                request.Price,
                request.Weight,
                request.Type,
                request.WarehouseNumber
            );

            var id = await productService.CreateAsync(
                command,
                cancellationToken
            );

            var response = new CreateProductResponse(id);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                response
            );
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
            [FromQuery] GetProductsRequest request,
            CancellationToken cancellationToken)
        {
            var filter = request.ToFilter();

            var result = await productService.GetAllAsync(
                filter,
                cancellationToken
            );

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var product = await productService.GetByIdAsync(
                id,
                cancellationToken
            );

            if (product is null)
            {
                return NotFound(new
                {
                    Message = $"Товар с Id {id} не найден."
                });
            }

            return Ok(product);
        }
    }
}
