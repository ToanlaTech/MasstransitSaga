using System.Reflection;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Models;
using MasstransitSaga.Core.StateMachine;
using MasstransitSaga.OrderSubmitService.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MasstransitSaga.Core.Environments;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IDatabaseSettings, DatabaseSettings>();
builder.Services.AddOptions<SqlTransportOptions>()
.Configure<IServiceProvider>((options, serviceProvider) =>
{
    var _dbSetting = serviceProvider.GetRequiredService<IDatabaseSettings>();
    options.ConnectionString = _dbSetting.GetPostgresConnectionString();
});
builder.Services.AddDbContext<WorldDbContext>((serviceProvider, options) =>
{
    var _dbSetting = serviceProvider.GetRequiredService<IDatabaseSettings>();
    options.UseNpgsql(_dbSetting.GetPostgresConnectionString());
});
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderSubmitConsumer>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
