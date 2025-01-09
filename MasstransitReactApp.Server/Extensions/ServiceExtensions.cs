using System;

namespace MasstransitReactApp.Server.Extensions;

public static class ServiceExtensions
{
  public static void AddSqlServerIdentityInfrastructure(this IServiceCollection services, string assembly)
  {
      var sp = services.BuildServiceProvider();
      using (var scope = sp.CreateScope())
      {
          var _dbSetting = scope.ServiceProvider.GetRequiredService<IDatabaseSettingsProvider>();
          string appConnStr = _dbSetting.GetSQLServerConnectionString();
          services.AddDbContext<IdentityContext>(options =>
              options.UseSqlServer(appConnStr,
              b =>
              {
                  b.MigrationsAssembly(assembly);
                  // b.MigrationsHistoryTable("__EFMigrationsHistory", "onion");
              }
          )
          );
      }
  }

  public static void AddMySqlIdentityInfrastructure(this IServiceCollection services, string assembly)
  {
      // Build the intermediate service provider
      var sp = services.BuildServiceProvider();
      using (var scope = sp.CreateScope())
      {
          var _dbSetting = scope.ServiceProvider.GetRequiredService<IDatabaseSettingsProvider>();
          string appConnStr = _dbSetting.GetMySQLConnectionString();
          if (!string.IsNullOrWhiteSpace(appConnStr))
          {
              var serverVersion = new MySqlServerVersion(new Version(5, 7, 35));
              services.AddDbContext<IdentityContext>(options =>
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

  public static void AddNpgSqlIdentityInfrastructure(this IServiceCollection services, string assembly)
  {
      // Build the intermediate service provider
      var sp = services.BuildServiceProvider();
      using (var scope = sp.CreateScope())
      {
          var _dbSetting = scope.ServiceProvider.GetRequiredService<IDatabaseSettingsProvider>();
          string appConnStr = _dbSetting.GetPostgresConnectionString();
          if (!string.IsNullOrWhiteSpace(appConnStr))
          {
              services.AddDbContext<IdentityContext>(options =>
              options.UseNpgsql(
              appConnStr,
              b =>
              {
                  b.MigrationsAssembly(assembly);
                  b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
              }));
          }
      }
  }
}
