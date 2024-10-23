using MassTransit;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;

namespace MasstransitSaga.OrderSubmitService.Consumers
{
    public class OrderSubmitConsumer : IConsumer<OrderSubmit>
    {
        private readonly OrderDbContext _dbContext;
        public OrderSubmitConsumer(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task Consume(ConsumeContext<OrderSubmit> context)
        {
            var message = context.Message;
            // delay 1s
            await Task.Delay(1000);
            // check product is exited in the database
            var product = await _dbContext.Products.FindAsync(message.ProductId);
            if (product == null)
            {
                await Task.WhenAll(
                    context.Publish<OrderCancel>(new
                    {
                        message.OrderId,
                        Reason = "Product is not found."
                    }),
                    context.Publish(new OrderResponse
                    (
                        message.OrderId,
                        "OrderRejected",
                        "Product is not found."
                    ))
                );
            }
            else
            {
                await Task.WhenAll(
                    context.Publish<OrderAccept>(new
                    {
                        message.OrderId,
                        message.ProductId,
                        message.Quantity
                    }),
                    context.Publish(new OrderResponse
                    (
                        message.OrderId,
                        "OrderAccepted",
                        "Product is accepted."
                    ))
                );
            }
        }
    }
}
