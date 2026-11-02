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

        Task<IReadOnlyCollection<Product>> GetAllAsync(
            CancellationToken cancellationToken = default
        );
    }
}
