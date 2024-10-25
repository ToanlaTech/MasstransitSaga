using MasstransitSaga.Core.StateMachine;
using MassTransit;
using MasstransitReactApp.Server.Consumers;
using MasstransitReactApp.Server.SignalRHubs;
using MasstransitSaga.Core.Environments;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using MasstransitSaga.Core.Context;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IDatabaseSettings, DatabaseSettings>();
// builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
builder.Services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
{
    var _dbSetting = serviceProvider.GetRequiredService<IDatabaseSettings>();
    options.UseNpgsql(_dbSetting.GetPostgresConnectionString());
});
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderSubmitConsumer>();
    x.AddConsumer<OrderAcceptConsumer>();
    x.AddConsumer<OrderCompleteConsumer>();
    x.AddConsumer<OrderReponseConsumer>();
    x.AddSagaStateMachine<OrderStateMachine, MasstransitSaga.Core.Models.Order>().RedisRepository(r =>
    {
        r.DatabaseConfiguration("localhost");
        // Default is Optimistic
        r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

        // Optional, prefix each saga instance key with the string specified
        // resulting dev:c6cfd285-80b2-4c12-bcd3-56a00d994736
        r.KeyPrefix = "dev";

        // Optional, to customize the lock key
        r.LockSuffix = "-lockage";

        // Optional, the default is 30 seconds
        r.LockTimeout = TimeSpan.FromSeconds(90);
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username("admin");
            h.Password("123456789");
        });
        cfg.ConfigureEndpoints(context);
    });
});
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
var app = builder.Build();
// ApplyMigrations(app);
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderStatusHub>("/hub/orderStatusHub");
app.MapFallbackToFile("/index.html");

app.Run();
