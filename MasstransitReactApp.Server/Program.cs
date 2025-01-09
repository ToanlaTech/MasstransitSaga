using MasstransitSaga.Core.Context;
using MasstransitReactApp.Server.SignalRHubs;
using Microsoft.EntityFrameworkCore;
using MasstransitSaga.Core.Environments;
using MasstransitReactApp.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IDatabaseSettings, DatabaseSettings>();

builder.Services.AddMySqlPersistenceInfrastructure(typeof(Program).Assembly.FullName);

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
        var dbContext = scope.ServiceProvider.GetRequiredService<WorldDbContext>();
        dbContext.Database.Migrate();
    }
}