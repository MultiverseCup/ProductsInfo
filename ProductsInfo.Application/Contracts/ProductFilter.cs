using ProductsInfo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Application.Contracts
{
    public sealed record ProductFilter(
    DateOnly? CreatedDate = null,
    ProductType? Type = null,
    int? WarehouseNumber = null,
    int Page = 1,
    int PageSize = 10
);
}
