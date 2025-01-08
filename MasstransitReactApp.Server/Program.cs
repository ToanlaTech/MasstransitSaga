using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Models;
using MasstransitSaga.Core.StateMachine;
using MassTransit;
using MasstransitReactApp.Server.Consumers;
using MasstransitReactApp.Server.SignalRHubs;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MasstransitSaga.Core.Environments;
using MasstransitReactApp.Server.Consumers.Todos;
using MasstransitReactApp.Server.Contracts.Todos;

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
    options.UseNpgsql(_dbSetting.GetPostgresConnectionString(), options =>
    {
        options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<GetTodosConsumer>(

    );
    x.AddConsumer<CreateTodoConsumer>();
    x.AddConsumer<UpdateTodoConsumer>();
    x.AddConsumer<DeleteTodoConsumer>();
    x.AddConsumer<GetTodoConsumer>();
    x.AddConsumer<TodoErrorConsumer>();
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
    // x.UsingPostgres((context, cfg) =>
    // {
    //     cfg.UseSqlMessageScheduler();

    //     // cfg.ReceiveEndpoint("get-todo-queue", e =>
    //     // {
    //     //     // Gắn Consumer
    //     //     e.ConfigureConsumer<GetTodoConsumer>(context);

    //     //     // Cấu hình Retry
    //     //     e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))); // Retry 3 lần

    //     //     // Kích hoạt cơ chế Dead Letter Queue mặc định
    //     //     e.DiscardFaultedMessages(); // Tự động chuyển message lỗi đến Dead Letter Queue mặc định
    //     // });

    //     // // Cấu hình Dead Letter Queue endpoint
    //     // cfg.ReceiveEndpoint("dead-letter-queue-get-todo", e =>
    //     // {
    //     //     // Consumer xử lý message lỗi trong Dead Letter Queue
    //     //     e.ConfigureConsumer<DeadLetterGetTodoConsumer>(context);
    //     // });

    //     // Cấu hình Receive Endpoint cho GetTodoConsumer
    //     cfg.ReceiveEndpoint("get-todo-queue", e =>
    //     {
    //         // Gắn Consumer GetTodoConsumer
    //         e.ConfigureConsumer<GetTodoConsumer>(context);

    //         // Cấu hình Retry Policy
    //         e.UseMessageRetry(r =>
    //         {
    //             r.Interval(3, TimeSpan.FromSeconds(5)); // Retry 3 lần, mỗi lần cách nhau 5 giây
    //         });

    //         // Cấu hình Dead Letter Queue
    //         e.DiscardFaultedMessages(); // Kích hoạt cơ chế Dead Letter Queue mặc định
    //     });

    //     // Cấu hình Receive Endpoint cho Dead Letter Queue
    //     cfg.ReceiveEndpoint("dead-letter-queue-get-todo", e =>
    //     {
    //         // Consumer xử lý message lỗi trong Dead Letter Queue
    //         e.ConfigureConsumer<DeadLetterGetTodoConsumer>(context);
    //     });

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

        cfg.ReceiveEndpoint("get-todo", e =>
        {
            e.ConfigureConsumer<GetTodoConsumer>(context);
            e.UseMessageRetry(r =>
            {
                r.Interval(3, TimeSpan.FromSeconds(5)); // Retry 3 lần, mỗi lần cách nhau 5 giây

                r.Handle<HttpRequestException>(); // Retry khi gặp lỗi HttpRequestException
                r.Handle<TimeoutException>(); // Retry khi gặp lỗi TimeoutException
                r.Handle<OperationCanceledException>(); // Retry khi gặp lỗi OperationCanceledException
            });
        });

        // Cấu hình Receive Endpoint cho Dead Letter Queue
        cfg.ReceiveEndpoint("get-todo-error", e =>
        {
            // Consumer xử lý message lỗi trong Dead Letter Queue
            e.ConfigureConsumer<TodoErrorConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });

    x.AddRequestClient<GetTodos>();
    x.AddRequestClient<CreateTodo>();
    x.AddRequestClient<UpdateTodo>();
    x.AddRequestClient<DeleteTodo>();
    x.AddRequestClient<GetTodo>();
});
builder.Services.AddHttpClient<CreateTodoConsumer>();
builder.Services.AddHttpClient<GetTodosConsumer>();
builder.Services.AddHttpClient<UpdateTodoConsumer>();
builder.Services.AddHttpClient<DeleteTodoConsumer>();
builder.Services.AddHttpClient<GetTodoConsumer>();
builder.Services.AddSingleton<AccountNumberProvider>();
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