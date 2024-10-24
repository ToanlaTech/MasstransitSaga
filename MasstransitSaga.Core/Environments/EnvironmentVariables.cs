using System;

namespace MasstransitSaga.Core.Environments;

public static class EnvironmentVariables
{
    public const string PostgresConnectionString = "POSTGRES_CONNECTION_STRING";
    public static bool HasPostgresConnectionString() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(PostgresConnectionString));
}
