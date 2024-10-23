using MassTransit;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Models;
using MasstransitSaga.Core.StateMachine;
using MasstransitSaga.OrderCompleteService.Consumers;
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
    x.AddConsumer<OrderCompleteConsumer>();
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
