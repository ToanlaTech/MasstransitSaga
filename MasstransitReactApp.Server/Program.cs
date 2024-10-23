using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Models;
using MasstransitSaga.Core.StateMachine;
using MassTransit;
using MasstransitReactApp.Server.Consumers;
using MasstransitReactApp.Server.SignalRHubs;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<SqlTransportOptions>()
    .Configure(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    });
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderSubmitConsumer>();
    x.AddConsumer<OrderAcceptConsumer>();
    x.AddConsumer<OrderCompleteConsumer>();
    x.AddConsumer<OrderReponseConsumer>();
    x.AddSagaStateMachine<OrderStateMachine, Order>()
    .EntityFrameworkRepository(r =>
    {
        r.ConcurrencyMode = ConcurrencyMode.Optimistic;
        r.AddDbContext<DbContext, OrderDbContext>((provider, b) =>
        {
            b.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOption =>
            {
                npgsqlOption.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                npgsqlOption.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

        });
    });

    x.AddSqlMessageScheduler();
    x.UsingPostgres((context, cfg) =>
    {
        cfg.UseSqlMessageScheduler();
        cfg.ConfigureEndpoints(context);
    });
});
// Add services to the container.
builder.Services.AddPostgresMigrationHostedService();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
var app = builder.Build();

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
