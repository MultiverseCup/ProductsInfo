using ProductsInfo.Application.Abstractions;
using ProductsInfo.Domain.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Infrastructure.Repositories
{
    public sealed class InMemoryProductRepository : IProductRepository
    {
        private readonly ConcurrentDictionary<Guid, Product> _products = new();

        public Task AddAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_products.TryAdd(product.Id, product))
            {
                throw new InvalidOperationException(
                    $"Товар с Id {product.Id} уже существует."
                );
            }

            return Task.CompletedTask;
        }

        public Task<Product?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _products.TryGetValue(id, out var product);

            return Task.FromResult(product);
        }

        public Task<IReadOnlyCollection<Product>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyCollection<Product> products = _products.Values.ToArray();

            return Task.FromResult(products);
        }
    }
}
