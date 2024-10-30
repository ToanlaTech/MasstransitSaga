using MassTransit;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;
using Microsoft.EntityFrameworkCore;
using RedLockNet;

namespace MasstransitSaga.OrderCompleteService.Consumers
{
    public class OrderCompleteConsumer : IConsumer<OrderComplete>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IDistributedLockFactory _redisLockFactory;

        public OrderCompleteConsumer(OrderDbContext dbContext, IDistributedLockFactory redisLockFactory)
        {
            _dbContext = dbContext;
            _redisLockFactory = redisLockFactory;
        }

        public async Task Consume(ConsumeContext<OrderComplete> context)
        {
            var message = context.Message;
            var productId = message.ProductId;
            var resource = $"lock:product:{productId}";
            var expiry = TimeSpan.FromSeconds(30); // Đặt thời gian khóa đủ dài để đảm bảo tiến trình hoàn thành

            Console.WriteLine($"Attempting to acquire Redis lock with key: {resource}");
            await Task.WhenAll(Console.Out.WriteLineAsync("#############################################"),
                    Console.Out.WriteLineAsync("Order Id: " + message.OrderId),
                    Console.Out.WriteLineAsync("#############################################"));
            await using (var redLock = await _redisLockFactory.CreateLockAsync(resource, expiry))
            {
                if (redLock.IsAcquired)
                {
                    Console.WriteLine($"Successfully acquired lock for {resource}");
                    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                    {
                        bool transactionCompleted = false;
                        try
                        {
                            var product = await _dbContext.Products
                                .FromSqlRaw("SELECT * FROM \"Products\" WHERE \"Id\" = {0} FOR UPDATE", productId)
                                .FirstOrDefaultAsync();

                            if (product != null)
                            {
                                product.Quantity -= message.Quantity;

                                await Task.WhenAll(
                                    _dbContext.SaveChangesAsync(),
                                    context.Publish(new OrderResponse(
                                        message.OrderId,
                                        "OrderCompleted",
                                        "Product is completed."
                                    ))
                                );

                                await transaction.CommitAsync();
                                transactionCompleted = true;
                                Console.WriteLine($"Order {message.OrderId} completed for ProductId: {productId}");
                            }
                            else
                            {
                                Console.WriteLine($"ProductId {productId} not found.");
                                await context.Publish(new OrderResponse(
                                    message.OrderId,
                                    "OrderFailed",
                                    "Product not found."
                                ));
                                await transaction.RollbackAsync();
                                transactionCompleted = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!transactionCompleted)
                            {
                                await transaction.RollbackAsync();
                            }
                            Console.WriteLine($"Error processing OrderComplete: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Could not acquire lock for ProductId: {productId}. Skipping processing.");
                }
            }
        }
    }
}
