using System;

namespace MasstransitSaga.Core.Environments;

public static class EnvironmentVariables
{
    public const string PostgresConnectionString = "POSTGRES_CONNECTION_STRING";
    public static bool HasPostgresConnectionString() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(PostgresConnectionString));
    public const string MySQLConnectionString = "MYSQL_CONNECTION_STRING";
    public static bool HasMySQLConnectionString() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(MySQLConnectionString));
    public const string SQLServerConnectionString = "SQLSERVER_CONNECTION_STRING";
    public static bool HasSQLServerConnectionString() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(SQLServerConnectionString));
}
