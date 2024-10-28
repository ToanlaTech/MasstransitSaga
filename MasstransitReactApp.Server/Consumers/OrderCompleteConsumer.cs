using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;
using MassTransit;

namespace MasstransitReactApp.Server.Consumers
{
    public class OrderCompleteConsumer : IConsumer<OrderComplete>
    {
        private readonly OrderDbContext _dbContext;
        public OrderCompleteConsumer(
            OrderDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }
        public async Task Consume(ConsumeContext<OrderComplete> context)
        {
            var message = context.Message;
            // Generate a random delay based on quantity
            Random random = new Random();
            int delay = random.Next(1, message.Quantity + 1) * 10; // Delay in milliseconds

            // Introduce the delay
            await Task.Delay(delay);
            // check product is exited in the database
            var product = await _dbContext.Products.FindAsync(message.ProductId);
            // minus product quantity
            product.Quantity -= message.Quantity;
            await Task.WhenAll(
                _dbContext.SaveChangesAsync(),
                context.Publish(new OrderResponse
                    (
                        message.OrderId,
                        "OrderCompleted",
                        "Product is completed."
                    ))
            );
        }
    }
}
