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

    #region RabbitMQ
    public const string RabbitMqHostName = "RABBITMQ_HOSTNAME";
    public static bool HasRabbitMqHostName() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RabbitMqHostName));
    public const string RabbitMqUserName = "RABBITMQ_USERNAME";
    public static bool HasRabbitMqUserName() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RabbitMqUserName));
    public const string RabbitMqPassword = "RABBITMQ_PASSWORD";
    public static bool HasRabbitMqPassword() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RabbitMqPassword));
    public const string RabbitMqVHost = "RABBITMQ_VHOST";
    public static bool HasRabbitMqVHost() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RabbitMqVHost));
    public const string RabbitMqPort = "RABBITMQ_PORT";
    public static bool HasRabbitMqPort() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RabbitMqPort));
    #endregion
}
