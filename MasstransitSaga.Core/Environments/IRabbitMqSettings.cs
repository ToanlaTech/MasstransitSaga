using System;
using MassTransit;
using RabbitMQ.Client;

namespace MasstransitSaga.Core.Environments;

public interface IRabbitMqSettings
{
    string GetHostName();
    string GetUserName();
    string GetPassword();
    string GetVHost();
    string GetPort();
    string GetConnectionString();
    bool IsHealthy();
    ConnectionFactory GetConnectionFactory();
    Task GetUri<T>(IBus _bus, string queueName, T message);
}
