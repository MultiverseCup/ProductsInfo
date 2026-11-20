using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Infrastructure.Migrations
{
    [Migration(2026111801, "Create products table")]
    public sealed class CreateProductsTable : Migration
    {
        public override void Up()
        {
            Execute.Sql("""
            CREATE TABLE products
            (
                id UUID PRIMARY KEY,

                name VARCHAR(200) NOT NULL,

                price NUMERIC(18, 2) NOT NULL
                    CHECK (price >= 0),

                weight NUMERIC(18, 3) NOT NULL
                    CHECK (weight > 0),

                product_type SMALLINT NOT NULL
                    CHECK (product_type BETWEEN 1 AND 4),

                created_at TIMESTAMPTZ NOT NULL
                    DEFAULT CURRENT_TIMESTAMP,

                warehouse_number INTEGER NOT NULL
                    CHECK (warehouse_number > 0)
            );

            CREATE INDEX ix_products_created_at
                ON products (created_at);

            CREATE INDEX ix_products_product_type
                ON products (product_type);

            CREATE INDEX ix_products_warehouse_number
                ON products (warehouse_number);
            """);
        }

        public override void Down()
        {
            Execute.Sql("""
            DROP TABLE IF EXISTS products;
            """);
        }
    }
}
