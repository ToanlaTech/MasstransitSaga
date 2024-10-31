using System.Net;
using MassTransit;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Environments;
using MasstransitSaga.OrderCompleteService.Consumers;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

var builder = WebApplication.CreateBuilder(args);
var _env = builder.Environment;
var redisConn = "localhost:6379";
if (_env.IsProduction())
{
    redisConn = Environment.GetEnvironmentVariable(EnvironmentVariables.RedisHost) ?? "localhost:6379";
}
builder.Services.AddTransient<IDatabaseSettings, DatabaseSettings>();
builder.Services.AddTransient<IRabbitMqSettings, RabbitMqSettings>();
builder.Services.AddOpenTelemetry()
.ConfigureResource(resource => resource.AddService(serviceName: "OrderSubmitService"))
.WithMetrics(metrics =>
  metrics
    .AddAspNetCoreInstrumentation() // ASP.NET Core related
    .AddRuntimeInstrumentation() // .NET Runtime metrics like - GC, Memory Pressure, Heap Leaks etc
    .AddPrometheusExporter() // Prometheus Exporter
);
var redisHost = redisConn.Split(':')[0];
var redisPort = int.Parse(redisConn.Split(':')[1]);
var redisEndpoints = new List<RedLockEndPoint>
{
    new DnsEndPoint(redisHost, redisPort)
};
var redisLockFactory = RedLockFactory.Create(redisEndpoints);
builder.Services.AddSingleton<IDistributedLockFactory>(redisLockFactory);
builder.Services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
{
    var _dbSetting = serviceProvider.GetRequiredService<IDatabaseSettings>();
    options.UseNpgsql(_dbSetting.GetPostgresConnectionString());
});
// Kiểm tra kết nối Redis
try
{
    using (var redLock = redisLockFactory.CreateLock("test-connection", TimeSpan.FromSeconds(1)))
    {
        if (redLock.IsAcquired)
        {
            Console.WriteLine("Kết nối Redis thành công và khóa phân tán đã được tạo.");
        }
        else
        {
            Console.WriteLine("Không thể tạo khóa phân tán - kiểm tra kết nối Redis.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Lỗi khi kết nối với Redis: {ex.Message}");
}
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCompleteConsumer>()
    .Endpoint(e =>
    {
        e.PrefetchCount = 1;
        e.ConcurrentMessageLimit = 1;
    });

    x.AddSqlMessageScheduler();
    x.UsingRabbitMq((context, cfg) =>
    {
        var _rabbitMqSetting = context.GetRequiredService<IRabbitMqSettings>();
        cfg.Host("rabbitmq://" + _rabbitMqSetting.GetHostName(), h =>
        {
            h.Username(_rabbitMqSetting.GetUserName());
            h.Password(_rabbitMqSetting.GetPassword());
        });
        cfg.ConfigureEndpoints(context);
    });
});
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Map the /metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
