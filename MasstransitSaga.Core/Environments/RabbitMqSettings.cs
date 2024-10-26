using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace MasstransitSaga.Core.Environments;

public class RabbitMqSettings : IRabbitMqSettings
{
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;
    public RabbitMqSettings(
        IHostEnvironment env,
        IConfiguration config
        )
    {
        _config = config;
        _env = env;
    }

    public ConnectionFactory GetConnectionFactory()
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(GetConnectionString())
        };
        return connectionFactory;
    }

    public string GetConnectionString()
    {

        return $"amqp://{GetUserName()}:{GetPassword()}@{GetHostName()}:{GetPort()}/{GetVHost()}";
    }

    public string GetHostName()
    {
        var isHasRabbitMqHostName = EnvironmentVariables.HasRabbitMqHostName();
        if (_env.IsProduction() && isHasRabbitMqHostName)
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.RabbitMqHostName) ?? string.Empty;
        }
        return _config["RabbitMq:Host"] ?? string.Empty;
    }

    public string GetPassword()
    {
        var isHasRabbitMqPassword = EnvironmentVariables.HasRabbitMqPassword();
        if (_env.IsProduction() && isHasRabbitMqPassword)
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.RabbitMqPassword) ?? string.Empty;
        }
        return _config["RabbitMq:Password"] ?? string.Empty;
    }

    public string GetPort()
    {
        var isHasRabbitMqPort = EnvironmentVariables.HasRabbitMqPort();
        if (_env.IsProduction() && isHasRabbitMqPort)
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.RabbitMqPort) ?? string.Empty;
        }
        return _config["RabbitMq:Port"] ?? string.Empty;
    }

    public async Task GetUri<T>(IBus _bus, string queueName, T message)
    {
        Uri uri;
        ISendEndpoint endPoint;
        if (!string.IsNullOrEmpty(GetVHost()) && GetVHost().Length > 0)
        {
            uri = new Uri($"rabbitmq://{GetHostName()}/{GetVHost()}/{queueName}");
        }
        else
        {
            uri = new Uri($"rabbitmq://{GetHostName()}/{queueName}");
        }

        endPoint = await _bus.GetSendEndpoint(uri);
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }
        await endPoint.Send(message);
    }

    public string GetUserName()
    {
        var isHasRabbitMqUserName = EnvironmentVariables.HasRabbitMqUserName();
        if (_env.IsProduction() && isHasRabbitMqUserName)
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.RabbitMqUserName) ?? string.Empty;
        }
        return _config["RabbitMq:UserName"] ?? string.Empty;
    }

    public string GetVHost()
    {
        var isHasRabbitMqVHost = EnvironmentVariables.HasRabbitMqVHost();
        if (_env.IsProduction() && isHasRabbitMqVHost)
        {
            return Environment.GetEnvironmentVariable(EnvironmentVariables.RabbitMqVHost) ?? string.Empty;
        }
        return _config["RabbitMq:VHost"] ?? string.Empty;
    }

    public bool IsHealthy()
    {
        try
        {
            var connectionFactory = GetConnectionFactory();
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Kiểm tra kết nối tới RabbitMQ bằng cách khởi tạo kết nối và kênh
                return connection.IsOpen && channel.IsOpen;
            }
        }
        catch (BrokerUnreachableException)
        {
            // Xử lý lỗi khi không kết nối được tới RabbitMQ
            return false;
        }
    }
}
