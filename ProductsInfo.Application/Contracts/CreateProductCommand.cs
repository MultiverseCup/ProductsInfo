using ProductsInfo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Application.Contracts
{
    public sealed record CreateProductCommand(
    string Name,
    decimal Price,
    decimal Weight,
    ProductType Type,
    int WarehouseNumber
    );
}
