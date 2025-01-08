using System;

namespace MasstransitSaga.Core.Environments;

public static class EnvironmentVariables
{
    public const string PostgresConnectionString = "POSTGRES_CONNECTION_STRING";
    public static bool HasPostgresConnectionString() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(PostgresConnectionString));

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
