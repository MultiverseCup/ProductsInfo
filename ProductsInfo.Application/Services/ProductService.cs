using FluentValidation;
using ProductsInfo.Application.Abstractions;
using ProductsInfo.Application.Contracts;
using ProductsInfo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Application.Services
{
    public sealed class ProductService(
    IProductRepository repository,
    IValidator<CreateProductCommand> createValidator,
    IValidator<ProductFilter> filterValidator
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

            await repository.AddAsync(product, cancellationToken);

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

            var products = await repository.GetAllAsync(cancellationToken);

            if (filter.CreatedDate.HasValue)
            {
                products = products
                    .Where(p => DateOnly.FromDateTime(p.CreatedAt.UtcDateTime)
                        == filter.CreatedDate.Value)
                    .ToArray();
            }

            if (filter.Type.HasValue)
            {
                products = products
                    .Where(p => p.Type == filter.Type.Value)
                    .ToArray();
            }

            if (filter.WarehouseNumber.HasValue)
            {
                products = products
                    .Where(p => p.WarehouseNumber == filter.WarehouseNumber.Value)
                    .ToArray();
            }

            var sortedProducts = products
                .OrderByDescending(p => p.CreatedAt)
                .ToArray();

            var totalCount = sortedProducts.Length;

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)filter.PageSize
            );

            var pagedProducts = sortedProducts
                .Skip((int)((long)filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(ToDto)
                .ToArray();

            return new PagedResult<ProductDto>(
                Items: pagedProducts,
                Page: filter.Page,
                PageSize: filter.PageSize,
                TotalCount: totalCount,
                TotalPages: totalPages
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

            if (product is null)
                return null;

            return ToDto(product);
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
