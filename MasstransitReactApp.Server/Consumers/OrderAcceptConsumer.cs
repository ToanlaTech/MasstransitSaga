using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;
using MassTransit;

namespace MasstransitReactApp.Server.Consumers
{
    public class OrderAcceptConsumer : IConsumer<OrderAccept>
    {
        private readonly WorldDbContext _dbContext;
        public OrderAcceptConsumer(
            WorldDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }
        public async Task Consume(ConsumeContext<OrderAccept> context)
        {
            var message = context.Message;
            // delay 1s
            await Task.Delay(2000);
            // check product quantity
            var product = await _dbContext.Products.FindAsync(message.ProductId);
            if (product.Quantity < message.Quantity)
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
