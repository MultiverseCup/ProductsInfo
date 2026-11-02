using ProductsInfo.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Application.Abstractions
{
    public interface IProductService
    {
        Task<Guid> CreateAsync(
            CreateProductCommand command,
            CancellationToken cancellationToken = default
        );

        Task<PagedResult<ProductDto>> GetAllAsync(
            ProductFilter filter,
            CancellationToken cancellationToken = default
        );

        Task<ProductDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );
    }
}
