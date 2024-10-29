using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MasstransitReactApp.Server.Consumers
{
    public class OrderAcceptConsumer : IConsumer<OrderAccept>
    {
        private readonly OrderDbContext _dbContext;
        public OrderAcceptConsumer(
            OrderDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }
        public async Task Consume(ConsumeContext<OrderAccept> context)
        {
            var message = context.Message;
            // Generate a random delay based on quantity
            Random random = new Random();
            int delay = random.Next(1, message.Quantity + 1) * 10; // Delay in milliseconds

            // Introduce the delay
            await Task.Delay(delay);
            // check product quantity
            // Check product quantity with row-level locking (FOR UPDATE)
            var product = await _dbContext.Products
                .FromSqlRaw("SELECT * FROM \"Products\" WHERE \"Id\" = {0} FOR UPDATE", message.ProductId)
                .FirstOrDefaultAsync();

            if (product == null || product.Quantity < message.Quantity)
            {
                await Task.WhenAll(
                    context.Publish<OrderCancel>(new
                    {
                        message.OrderId,
                    }),
                    context.Publish(new OrderResponse
                    (
                        message.OrderId,
                        "OrderRejected",
                        "Product is out of stock."
                    ))
                );
            }
            else
            {
                // Tùy chọn: Gửi sự kiện rằng đơn hàng đã được hoàn thành
                await context.Publish<OrderComplete>(new
                {
                    message.OrderId,
                    message.ProductId,
                    message.Quantity
                });
            }
        }
    }
}
