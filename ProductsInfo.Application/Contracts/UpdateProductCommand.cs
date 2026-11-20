using ProductsInfo.Domain.Enums;

namespace ProductsInfo.Application.Contracts
{
    public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    decimal Price,
    decimal Weight,
    ProductType Type,
    int WarehouseNumber
    );
}
