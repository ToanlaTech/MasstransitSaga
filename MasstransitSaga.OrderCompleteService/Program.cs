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
