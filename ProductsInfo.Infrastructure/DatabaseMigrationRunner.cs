using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsInfo.Infrastructure
{
    public static class DatabaseMigrationRunner
    {
        public static void ApplyMigrations(
            this IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var runner = scope.ServiceProvider
                .GetRequiredService<IMigrationRunner>();

            runner.MigrateUp();
        }
    }
}
