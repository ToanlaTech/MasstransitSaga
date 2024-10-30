using MassTransit;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;
using Microsoft.EntityFrameworkCore;

namespace MasstransitSaga.OrderAcceptService.Consumers
{
    public class OrderAcceptConsumer : IConsumer<OrderAccept>
    {
        private readonly OrderDbContext _dbContext;

        public OrderAcceptConsumer(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<OrderAccept> context)
        {
            var message = context.Message;
            // Generate a random delay from 100 to 500 ms
            var delay = new Random().Next(100, 500);
            await Task.Delay(delay);
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                bool transactionCompleted = false;
                try
                {
                    var product = await _dbContext.Products
                                        .FromSqlRaw("SELECT * FROM \"Products\" WHERE \"Id\" = {0} FOR UPDATE", message.ProductId)
                                        .FirstOrDefaultAsync();

                    if (product == null || product.Quantity < message.Quantity)
                    {
                        await Task.WhenAll(
                            context.Publish<OrderCancel>(new
                            {
                                OrderId = message.OrderId,
                            }),
                            context.Publish(new OrderResponse
                            (
                                message.OrderId,
                                "OrderRejected",
                                "Product is out of stock."
                            ))
                        );
                        // Rollback và ném lỗi nếu không đủ số lượng
                        await transaction.RollbackAsync();
                        transactionCompleted = true;
                    }
                    else
                    {
                        // Commit transaction nếu số lượng sản phẩm đủ
                        await transaction.CommitAsync();
                        transactionCompleted = true;
                        Console.WriteLine("Product quantity is sufficient for the order.");
                        // Tùy chọn: Gửi sự kiện rằng đơn hàng đã được hoàn thành
                        await context.Publish<OrderComplete>(new
                        {
                            message.OrderId,
                            message.ProductId,
                            message.Quantity
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Rollback trong trường hợp có lỗi
                    if (!transactionCompleted)
                    {
                        // Rollback chỉ khi transaction chưa hoàn tất
                        await transaction.RollbackAsync();
                    }
                    Console.WriteLine($"Error processing OrderAccept: {ex.Message}");
                }


            }
        }
    }
}
