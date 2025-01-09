using System;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Environments;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace MasstransitReactApp.Server.Extensions;

public static class ServiceExtensions
{
  
    public static void AddMySqlPersistenceInfrastructure(this IServiceCollection services, string assembly)
    {
        // Build the intermediate service provider
        var sp = services.BuildServiceProvider();
        using (var scope = sp.CreateScope())
        {
            var _dbSetting = scope.ServiceProvider.GetRequiredService<IDatabaseSettings>();
            string appConnStr = _dbSetting.GetMySQLConnectionString();
            if (!string.IsNullOrWhiteSpace(appConnStr))
            {
                var serverVersion = new MySqlServerVersion(new Version(5, 7, 35));
                services.AddDbContext<WorldDbContext>(options =>
                options.UseMySql(
                    appConnStr, serverVersion,
                    b =>
                    {
                        b.SchemaBehavior(MySqlSchemaBehavior.Ignore);
                        b.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        b.MigrationsAssembly(assembly);
                        b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }));
            }
        }
    }
  
    public static void AddNpgSqlPersistenceInfrastructure(this IServiceCollection services, string assembly)
    {
        // Build the intermediate service provider
        var sp = services.BuildServiceProvider();
        using (var scope = sp.CreateScope())
        {
            var _dbSetting = scope.ServiceProvider.GetRequiredService<IDatabaseSettings>();
            string appConnStr = _dbSetting.GetPostgresConnectionString();
            if (!string.IsNullOrWhiteSpace(appConnStr))
            {
                services.AddDbContext<WorldDbContext>((options) =>
                options.UseNpgsql(
                appConnStr,
                b =>
                {
                    b.MigrationsAssembly(assembly);
                    b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
  
                );
            }
        }
    }
}
