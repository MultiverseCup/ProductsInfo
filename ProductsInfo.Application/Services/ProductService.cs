using FluentValidation;
using ProductsInfo.Application.Abstractions;
using ProductsInfo.Application.Contracts;
using ProductsInfo.Domain.Entities;

namespace ProductsInfo.Application.Services
{
    public sealed class ProductService(
    IProductRepository repository,
    IValidator<CreateProductCommand> createValidator,
    IValidator<ProductFilter> filterValidator,
    IValidator<UpdateProductCommand> updateValidator
    ) : IProductService
    {
        public async Task<Guid> CreateAsync(
            CreateProductCommand command,
            CancellationToken cancellationToken = default)
        {
            await createValidator.ValidateAndThrowAsync(
                command,
                cancellationToken
            );

            var product = new Product(
                Id: Guid.NewGuid(),
                Name: command.Name.Trim(),
                Price: command.Price,
                Weight: command.Weight,
                Type: command.Type,
                CreatedAt: DateTimeOffset.UtcNow,
                WarehouseNumber: command.WarehouseNumber
            );

            await repository.AddAsync(
                product,
                cancellationToken
            );

            return product.Id;
        }

        public async Task<PagedResult<ProductDto>> GetAllAsync(
            ProductFilter filter,
            CancellationToken cancellationToken = default)
        {
            await filterValidator.ValidateAndThrowAsync(
                filter,
                cancellationToken
            );

            var result = await repository.GetPagedAsync(
                filter,
                cancellationToken
            );

            var items = result.Items
                .Select(ToDto)
                .ToArray();

            return new PagedResult<ProductDto>(
                Items: items,
                Page: result.Page,
                PageSize: result.PageSize,
                TotalCount: result.TotalCount,
                TotalPages: result.TotalPages
            );
        }

        public async Task<ProductDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var product = await repository.GetByIdAsync(
                id,
                cancellationToken
            );

            return product is null
                ? null
                : ToDto(product);
        }

        public async Task<bool> UpdateAsync(
            UpdateProductCommand command,
            CancellationToken cancellationToken = default)
        {
            await updateValidator.ValidateAndThrowAsync(
                command,
                cancellationToken
            );

            var existingProduct = await repository.GetByIdAsync(
                command.Id,
                cancellationToken
            );

            if (existingProduct is null)
                return false;

            var updatedProduct = existingProduct with
            {
                Name = command.Name.Trim(),
                Price = command.Price,
                Weight = command.Weight,
                Type = command.Type,
                WarehouseNumber = command.WarehouseNumber
            };

            return await repository.UpdateAsync(
                updatedProduct,
                cancellationToken
            );
        }

        private static ProductDto ToDto(Product product)
        {
            return new ProductDto(
                Id: product.Id,
                Name: product.Name,
                Price: product.Price,
                Weight: product.Weight,
                Type: product.Type,
                CreatedAt: product.CreatedAt,
                WarehouseNumber: product.WarehouseNumber
            );
        }
    }
}
