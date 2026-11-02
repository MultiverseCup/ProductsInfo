using ProductsInfo.Application.Contracts;
using ProductsInfo.Domain.Enums;

namespace ProductsInfo.Presentation.Models
{
    public sealed class GetProductsRequest
    {
        public DateOnly? CreatedDate { get; set; }

        public ProductType? Type { get; set; }

        public int? WarehouseNumber { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public ProductFilter ToFilter()
        {
            return new ProductFilter(
                CreatedDate,
                Type,
                WarehouseNumber,
                Page,
                PageSize
            );
        }
    }
}
