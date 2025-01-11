using MasstransitSaga.Core.Context;
using MasstransitReactApp.Server.SignalRHubs;
using Microsoft.EntityFrameworkCore;
using MasstransitSaga.Core.Environments;
using MasstransitReactApp.Server.Extensions;
using MassTransit;
using Prometheus;
using MasstransitReactApp.Server.Consumers.Todos;
using MasstransitReactApp.Server.Contracts.Todos;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IDatabaseSettings, DatabaseSettings>();
builder.Services.AddTransient<IRabbitMqSettings, RabbitMqSettings>();

builder.Services.AddMySqlPersistenceInfrastructure(typeof(Program).Assembly.FullName);
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<GetTodosConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var _rabbitMqSetting = context.GetRequiredService<IRabbitMqSettings>();
        cfg.Host("rabbitmq://" + _rabbitMqSetting.GetHostName(), h =>
        {
            h.Username(_rabbitMqSetting.GetUserName());
            h.Password(_rabbitMqSetting.GetPassword());
        });
        cfg.ReceiveEndpoint("get-todos", e =>
        {
            e.ConfigureConsumer<GetTodosConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
    x.AddRequestClient<GetTodos>();
});
builder.Services.AddHttpClient<GetTodosConsumer>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
var app = builder.Build();
ApplyMigrations(app);
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpMetrics(); // Thu thập metrics HTTP request

// Định nghĩa endpoint /metrics
app.MapMetrics();
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

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
        var dbContext = scope.ServiceProvider.GetRequiredService<WorldDbContext>();
        dbContext.Database.Migrate();
    }
}