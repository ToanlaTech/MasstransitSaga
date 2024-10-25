using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Models;
using MasstransitSaga.Core.StateMachine;
using MassTransit;
using MasstransitReactApp.Server.Consumers;
using MasstransitReactApp.Server.SignalRHubs;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MasstransitSaga.Core.Environments;

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
    options.UseNpgsql(_dbSetting.GetPostgresConnectionString(), options =>
    {
        options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

builder.Services.AddMassTransit(x =>
{
    // x.AddConsumer<OrderSubmitConsumer>();
    // x.AddConsumer<OrderAcceptConsumer>();
    // x.AddConsumer<OrderCompleteConsumer>();
    x.AddConsumer<OrderReponseConsumer>();
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
ApplyMigrations(app);
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

void ApplyMigrations(IHost app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        dbContext.Database.Migrate();
    }
}