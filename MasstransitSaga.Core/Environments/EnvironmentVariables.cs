using System;

namespace MasstransitSaga.Core.Environments;

public static class EnvironmentVariables
{
    public const string PostgresConnectionString = "POSTGRES_CONNECTION_STRING";
    public static bool HasPostgresConnectionString() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(PostgresConnectionString));

    #region Redis Cache
    public const string RedisHost = "REDIS_HOST";
    public static bool HasRedisHost() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RedisHost));
    public const string RedisPort = "REDIS_PORT";
    public static bool HasRedisPort() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RedisPort));
    public const string RedisPassword = "REDIS_PASSWORD";
    public static bool HasRedisPassword() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RedisPassword));
    #endregion

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
