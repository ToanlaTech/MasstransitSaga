using MasstransitSaga.Core.StateMachine;
using MassTransit;
using MasstransitReactApp.Server.Consumers;
using MasstransitReactApp.Server.SignalRHubs;
using MasstransitSaga.Core.Environments;
using Microsoft.EntityFrameworkCore;
using MasstransitSaga.Core.Context;
using System.Reflection;
using MasstransitReactApp.Server;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var _env = builder.Environment;
var redisConn = "localhost:6379";
if (_env.IsProduction())
{
    redisConn = Environment.GetEnvironmentVariable(EnvironmentVariables.RedisHost) ?? "localhost:6379";
}
builder.Services.AddTransient<IDatabaseSettings, DatabaseSettings>();
builder.Services.AddSingleton<IRedisSettings, RedisSettings>();
builder.Services.AddTransient<IRabbitMqSettings, RabbitMqSettings>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
{
    var _dbSetting = serviceProvider.GetRequiredService<IDatabaseSettings>();
    options.UseNpgsql(_dbSetting.GetPostgresConnectionString(), options =>
    {
        options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});
builder.Services.AddMassTransit(x =>
{
    // x.AddConsumer<OrderSubmitConsumer>()
    // .Endpoint(e =>
    // {
    //     e.PrefetchCount = 1;
    //     e.ConcurrentMessageLimit = 1;
    // });
    // x.AddConsumer<OrderAcceptConsumer>()
    // .Endpoint(e =>
    // {
    //     e.PrefetchCount = 1;
    //     e.ConcurrentMessageLimit = 1;
    // });
    // x.AddConsumer<OrderCompleteConsumer>()
    // .Endpoint(e =>
    // {
    //     e.PrefetchCount = 1;
    //     e.ConcurrentMessageLimit = 1;
    // });
    x.AddConsumer<OrderReponseConsumer>()
    .Endpoint(e =>
    {
        e.PrefetchCount = 1;
        e.ConcurrentMessageLimit = 1;
    });
    x.AddSagaStateMachine<OrderStateMachine, MasstransitSaga.Core.Models.Order>().RedisRepository(r =>
    {
        r.DatabaseConfiguration(redisConn);
        // Default is Optimistic
        r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

        // Optional, prefix each saga instance key with the string specified
        // resulting dev:c6cfd285-80b2-4c12-bcd3-56a00d994736
        r.KeyPrefix = "dev";

        // Optional, to customize the lock key
        r.LockSuffix = "-lockage";

        // Optional, the default is 30 seconds
        r.LockTimeout = TimeSpan.FromSeconds(90);

        // expire the saga instance after 5 minutes
    });

    // Thêm middleware để đặt TTL cho các khóa saga trong Redis
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
builder.Services.AddHostedService<OrderSyncService>();
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
var app = builder.Build();
ApplyMigrations(app);
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderStatusHub>("/hub/orderStatusHub");
app.MapFallbackToFile("/index.html");

app.Run();

void ApplyMigrations(IHost app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        dbContext.Database.Migrate();
    }
}