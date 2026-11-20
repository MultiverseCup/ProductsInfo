using ProductsInfo.Application.Contracts;
using ProductsInfo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Application.Abstractions
{
    public interface IProductRepository
    {
        Task AddAsync(
            Product product,
            CancellationToken cancellationToken = default
        );

        Task<Product?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );

        Task<PagedResult<Product>> GetPagedAsync(
            ProductFilter filter,
            CancellationToken cancellationToken = default
        );

        Task<bool> UpdateAsync(
            Product product,
            CancellationToken cancellationToken = default
        );
    }
}
