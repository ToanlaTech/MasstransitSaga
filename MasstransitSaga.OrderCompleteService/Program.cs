using MassTransit;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Environments;
using MasstransitSaga.Core.Models;
using MasstransitSaga.Core.StateMachine;
using MasstransitSaga.OrderCompleteService.Consumers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IDatabaseSettings, DatabaseSettings>();
builder.Services.AddTransient<IRabbitMqSettings, RabbitMqSettings>();
builder.Services.AddOptions<SqlTransportOptions>()
.Configure<IServiceProvider>((options, serviceProvider) =>
{
    var _dbSetting = serviceProvider.GetRequiredService<IDatabaseSettings>();
    options.ConnectionString = _dbSetting.GetPostgresConnectionString();
});
builder.Services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
{
    var _dbSetting = serviceProvider.GetRequiredService<IDatabaseSettings>();
    options.UseNpgsql(_dbSetting.GetPostgresConnectionString());
});
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCompleteConsumer>();
    x.AddSagaStateMachine<OrderStateMachine, Order>()
    .EntityFrameworkRepository(r =>
    {
        r.ConcurrencyMode = ConcurrencyMode.Optimistic;
        r.AddDbContext<DbContext, OrderDbContext>((provider, b) =>
        {
            var _dbSetting = provider.GetRequiredService<IDatabaseSettings>();
            b.UseNpgsql(_dbSetting.GetPostgresConnectionString(), npgsqlOption =>
            {
                npgsqlOption.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                npgsqlOption.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

        });
    });

    // x.AddSqlMessageScheduler();
    // x.UsingPostgres((context, cfg) =>
    // {
    //     cfg.UseSqlMessageScheduler();
    //     cfg.ConfigureEndpoints(context);
    // });
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
