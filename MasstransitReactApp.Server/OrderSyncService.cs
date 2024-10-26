using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Models;
using Newtonsoft.Json;
using EFCore.BulkExtensions;
using MasstransitSaga.Core.Environments;

namespace MasstransitReactApp.Server;

public class OrderSyncService : BackgroundService
{
    private readonly IHostEnvironment _env;
    private readonly StackExchange.Redis.IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;

    public OrderSyncService(
        IHostEnvironment env,
        StackExchange.Redis.IConnectionMultiplexer redis,
        IServiceProvider serviceProvider)
    {
        _env = env;
        _redis = redis;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncOrdersFromRedisToPostgres();
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Đồng bộ mỗi 5 phút
        }
    }

    private async Task SyncOrdersFromRedisToPostgres()
    {
        var redisConn = "localhost:6379";
        if (_env.IsProduction())
        {
            redisConn = Environment.GetEnvironmentVariable(EnvironmentVariables.RedisHost) ?? "localhost:6379";
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            var db = _redis.GetDatabase();
            var server = _redis.GetServer(redisConn);
            var keys = server.Keys(pattern: "dev:*");

            var orders = new List<Order>();

            foreach (var key in keys)
            {
                var orderData = await db.StringGetAsync(key);
                var order = JsonConvert.DeserializeObject<Order>(orderData);
                orders.Add(order);

                // Xóa order trong Redis sau khi đã đưa vào danh sách đồng bộ
                // await db.KeyDeleteAsync(key);
            }

            // Bulk insert hoặc update toàn bộ danh sách order trong PostgreSQL
            await dbContext.BulkInsertOrUpdateAsync(orders);
        }
    }
}
