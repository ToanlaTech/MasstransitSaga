using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;
using MassTransit;

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
            var product = await _dbContext.Products.FindAsync(message.ProductId);
            if (product.Quantity < message.Quantity)
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
