using Dapper;
using Npgsql;
using ProductsInfo.Application.Abstractions;
using ProductsInfo.Application.Contracts;
using ProductsInfo.Domain.Entities;
using ProductsInfo.Domain.Enums;

namespace ProductsInfo.Infrastructure.Repositories
{
    public sealed class PostgresProductRepository(
    NpgsqlDataSource dataSource
) : IProductRepository
    {
        public async Task AddAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
            INSERT INTO products
            (
                id,
                name,
                price,
                weight,
                product_type,
                created_at,
                warehouse_number
            )
            VALUES
            (
                @Id,
                @Name,
                @Price,
                @Weight,
                @Type,
                @CreatedAt,
                @WarehouseNumber
            );
            """;

            await using var connection =
                await dataSource.OpenConnectionAsync(cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        product.Id,
                        product.Name,
                        product.Price,
                        product.Weight,
                        Type = (short)product.Type,
                        product.CreatedAt,
                        product.WarehouseNumber
                    },
                    cancellationToken: cancellationToken
                )
            );
        }

        public async Task<Product?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
            SELECT
                id AS "Id",
                name AS "Name",
                price AS "Price",
                weight AS "Weight",
                product_type AS "Type",
                created_at AS "CreatedAt",
                warehouse_number AS "WarehouseNumber"
            FROM products
            WHERE id = @Id;
            """;

            await using var connection =
                await dataSource.OpenConnectionAsync(cancellationToken);

            var row = await connection.QuerySingleOrDefaultAsync<ProductDbRow>(
                new CommandDefinition(
                    sql,
                    new { Id = id },
                    cancellationToken: cancellationToken
                )
            );

            return row is null ? null : ToProduct(row);
        }

        public async Task<PagedResult<Product>> GetPagedAsync(
            ProductFilter filter,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await dataSource.OpenConnectionAsync(cancellationToken);

            var conditions = new List<string>();

            var parameters = new DynamicParameters();

            if (filter.CreatedDate.HasValue)
            {
                var date = filter.CreatedDate.Value;

                var utcDateTime = DateTime.SpecifyKind(
                    date.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc
                );

                var dateFrom = new DateTimeOffset(utcDateTime);
                var dateTo = dateFrom.AddDays(1);

                conditions.Add(
                    "created_at >= @DateFrom AND created_at < @DateTo"
                );

                parameters.Add("DateFrom", dateFrom);
                parameters.Add("DateTo", dateTo);
            }

            if (filter.Type.HasValue)
            {
                conditions.Add("product_type = @Type");

                parameters.Add(
                    "Type",
                    (short)filter.Type.Value
                );
            }

            if (filter.WarehouseNumber.HasValue)
            {
                conditions.Add("warehouse_number = @WarehouseNumber");

                parameters.Add(
                    "WarehouseNumber",
                    filter.WarehouseNumber.Value
                );
            }

            var whereSql = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;

            var countSql = $"""
            SELECT COUNT(*)::integer
            FROM products
            {whereSql};
            """;

            var totalCount = await connection.QuerySingleAsync<int>(
                new CommandDefinition(
                    countSql,
                    parameters,
                    cancellationToken: cancellationToken
                )
            );

            var offset = ((long)filter.Page - 1) * filter.PageSize;

            parameters.Add("PageSize", filter.PageSize);
            parameters.Add("Offset", offset);

            var sql = $"""
            SELECT
                id AS "Id",
                name AS "Name",
                price AS "Price",
                weight AS "Weight",
                product_type AS "Type",
                created_at AS "CreatedAt",
                warehouse_number AS "WarehouseNumber"
            FROM products
            {whereSql}
            ORDER BY created_at DESC, id
            LIMIT @PageSize
            OFFSET @Offset;
            """;

            var rows = await connection.QueryAsync<ProductDbRow>(
                new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: cancellationToken
                )
            );

            var products = rows
                .Select(ToProduct)
                .ToArray();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)filter.PageSize
            );

            return new PagedResult<Product>(
                Items: products,
                Page: filter.Page,
                PageSize: filter.PageSize,
                TotalCount: totalCount,
                TotalPages: totalPages
            );
        }

        public async Task<bool> UpdateAsync(
            Product product,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
            UPDATE products
            SET
                name = @Name,
                price = @Price,
                weight = @Weight,
                product_type = @Type,
                warehouse_number = @WarehouseNumber
            WHERE id = @Id;
            """;

            await using var connection =
                await dataSource.OpenConnectionAsync(cancellationToken);

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        product.Id,
                        product.Name,
                        product.Price,
                        product.Weight,
                        Type = (short)product.Type,
                        product.WarehouseNumber
                    },
                    cancellationToken: cancellationToken
                )
            );

            return affectedRows > 0;
        }

        private sealed class ProductDbRow
        {
            public Guid Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public decimal Price { get; set; }

            public decimal Weight { get; set; }

            public short Type { get; set; }

            public DateTimeOffset CreatedAt { get; set; }

            public int WarehouseNumber { get; set; }
        }

        private static Product ToProduct(ProductDbRow row)
        {
            return new Product(
                Id: row.Id,
                Name: row.Name,
                Price: row.Price,
                Weight: row.Weight,
                Type: (ProductType)row.Type,
                CreatedAt: row.CreatedAt,
                WarehouseNumber: row.WarehouseNumber
            );
        }
    }
}
