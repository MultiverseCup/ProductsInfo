using Grpc.Core;
using ProductsInfo.Application.Abstractions;
using ProductsInfo.Application.Contracts;
using System.Globalization;

using DomainProductType = ProductsInfo.Domain.Enums.ProductType;
using ProtoProductType = ProductsInfo.Presentation.Protos.ProductType;
using Proto = ProductsInfo.Presentation.Protos;

namespace ProductsInfo.Presentation.Grpc;

public sealed class ProductsGrpcService(
    IProductService productService
) : Proto.Products.ProductsBase
{
    public override async Task<Proto.CreateProductResponse> CreateProduct(
        Proto.CreateProductRequest request,
        ServerCallContext context)
    {
        var price = ParseDecimal(request.Price, "Цена");

        var weight = ParseDecimal(request.Weight, "Вес");

        var type = ToDomainType(request.Type);

        var command = new CreateProductCommand(
            request.Name,
            price,
            weight,
            type,
            request.WarehouseNumber
        );

        var id = await productService.CreateAsync(
            command,
            context.CancellationToken
        );

        return new Proto.CreateProductResponse
        {
            Id = id.ToString()
        };
    }

    public override async Task<Proto.GetProductsResponse> GetProducts(
        Proto.GetProductsRequest request,
        ServerCallContext context)
    {
        DateOnly? createdDate = null;

        if (request.HasCreatedDate)
        {
            if (!DateOnly.TryParseExact(
                    request.CreatedDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        "Дата должна иметь формат yyyy-MM-dd."
                    )
                );
            }

            createdDate = parsedDate;
        }

        DomainProductType? type = null;

        if (request.HasType)
        {
            type = ToDomainType(request.Type);
        }

        int? warehouseNumber = request.HasWarehouseNumber
            ? request.WarehouseNumber
            : null;

        var page = request.Page == 0 ? 1 : request.Page;

        var pageSize = request.PageSize == 0
            ? 10
            : request.PageSize;

        var filter = new ProductFilter(
            CreatedDate: createdDate,
            Type: type,
            WarehouseNumber: warehouseNumber,
            Page: page,
            PageSize: pageSize
        );

        var result = await productService.GetAllAsync(
            filter,
            context.CancellationToken
        );

        var response = new Proto.GetProductsResponse
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };

        response.Products.AddRange(
            result.Items.Select(ToProto)
        );

        return response;
    }

    public override async Task<Proto.ProductMessage> GetProductById(
        Proto.GetProductByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "Передан некорректный идентификатор товара."
                )
            );
        }

        var product = await productService.GetByIdAsync(
            id,
            context.CancellationToken
        );

        if (product is null)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    $"Товар с Id {id} не найден."
                )
            );
        }

        return ToProto(product);
    }

    private static decimal ParseDecimal(
        string value,
        string fieldName)
    {
        if (!decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var result))
        {
            throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    $"{fieldName} имеет некорректный формат."
                )
            );
        }

        return result;
    }

    private static DomainProductType ToDomainType(
        ProtoProductType type)
    {
        return type switch
        {
            ProtoProductType.General =>
                DomainProductType.General,

            ProtoProductType.HouseholdChemicals =>
                DomainProductType.HouseholdChemicals,

            ProtoProductType.Electronics =>
                DomainProductType.Electronics,

            ProtoProductType.Food =>
                DomainProductType.Food,

            _ => throw new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    "Указан неизвестный вид товара."
                )
            )
        };
    }

    private static ProtoProductType ToGrpcType(
        DomainProductType type)
    {
        return type switch
        {
            DomainProductType.General =>
                ProtoProductType.General,

            DomainProductType.HouseholdChemicals =>
                ProtoProductType.HouseholdChemicals,

            DomainProductType.Electronics =>
                ProtoProductType.Electronics,

            DomainProductType.Food =>
                ProtoProductType.Food,

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static Proto.ProductMessage ToProto(ProductDto product)
    {
        return new Proto.ProductMessage
        {
            Id = product.Id.ToString(),
            Name = product.Name,

            Price = product.Price.ToString(
                CultureInfo.InvariantCulture
            ),

            Weight = product.Weight.ToString(
                CultureInfo.InvariantCulture
            ),

            Type = ToGrpcType(product.Type),

            CreatedAt = product.CreatedAt.ToString(
                "O",
                CultureInfo.InvariantCulture
            ),

            WarehouseNumber = product.WarehouseNumber
        };
    }
}