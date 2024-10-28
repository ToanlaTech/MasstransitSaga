using MassTransit;
using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;

namespace MasstransitReactApp.Server.Consumers.One;

public class OrderSubmitOneConsumer : IConsumer<OrderSubmit>
{
    private readonly OrderDbContext _dbContext;
    public OrderSubmitOneConsumer(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<OrderSubmit> context)
    {
        var message = context.Message;
        // Generate a random delay based on quantity
        Random random = new Random();
        int delay = random.Next(1, message.Quantity + 1) * 100; // Delay in milliseconds

        // Introduce the delay
        await Task.Delay(delay);
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
                    "Product is out of stock."
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
