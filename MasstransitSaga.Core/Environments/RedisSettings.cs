using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MasstransitSaga.Core.Environments;

public class RedisSettings : IRedisSettings
{
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _config;
    public RedisSettings(
        IHostEnvironment env,
        IConfiguration config
        )
    {
        _config = config;
        _env = env;
    }
    public string GetRedisConfiguration()
    {
        if (_env.IsDevelopment())
        {
            return _config.GetConnectionString("Redis") ?? "localhost:6379";
        }

        return Environment.GetEnvironmentVariable(EnvironmentVariables.RedisHost) ?? "localhost:6379";
    }
}
