using ProductsInfo.Domain.Enums;

namespace ProductsInfo.Presentation.Models
{
    public sealed record UpdateProductRequest(
    string Name,
    decimal Price,
    decimal Weight,
    ProductType Type,
    int WarehouseNumber
);
}
