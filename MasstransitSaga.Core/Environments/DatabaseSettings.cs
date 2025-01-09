using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MasstransitSaga.Core.Environments;

public class DatabaseSettings : IDatabaseSettings
{
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;
    public DatabaseSettings(
        IHostEnvironment env,
        IConfiguration config
        )
    {
        _env = env;
        _config = config;
    }

    public string GetPostgresConnectionString()
    {
        var isHasPostgresConnectionString = EnvironmentVariables.HasPostgresConnectionString();
        if (_env.IsProduction() && isHasPostgresConnectionString)
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.PostgresConnectionString) ?? string.Empty;
        }
        return _config.GetConnectionString("PostgresConnection") ?? string.Empty;
    }

    public string GetMySQLConnectionString()
    {

        var isHasMySQLConnectionString = EnvironmentVariables.HasMySQLConnectionString();
        if (_env.IsProduction() && isHasMySQLConnectionString)
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.MySQLConnectionString);
        }
        return _config.GetConnectionString("MySQLConnection");
    }

    public string GetSQLServerConnectionString()
    {
        var isHasSQLServerConnectionString = EnvironmentVariables.HasSQLServerConnectionString();
        if (_env.IsProduction() && isHasSQLServerConnectionString)
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.SQLServerConnectionString);
        }
        return _config.GetConnectionString("SQLServerConnection");
    }
}

public interface IDatabaseSettings
{
    string GetPostgresConnectionString();
    string GetMySQLConnectionString();
    string GetSQLServerConnectionString();
}
