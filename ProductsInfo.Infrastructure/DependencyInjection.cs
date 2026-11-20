using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ProductsInfo.Application.Abstractions;
using ProductsInfo.Infrastructure.Migrations;
using ProductsInfo.Infrastructure.Repositories;

namespace ProductsInfo.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddSingleton<NpgsqlDataSource>(
                _ => NpgsqlDataSource.Create(connectionString)
            );

            services.AddScoped<
                IProductRepository,
                PostgresProductRepository
            >();

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(runner => runner
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(CreateProductsTable).Assembly)
                    .For.Migrations()
                )
                .AddLogging(logging =>
                    logging.AddFluentMigratorConsole()
                );

            return services;
        }
    }
}
