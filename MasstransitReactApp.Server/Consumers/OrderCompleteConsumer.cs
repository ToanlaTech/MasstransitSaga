﻿using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.StateMachine;
using MassTransit;

namespace MasstransitReactApp.Server.Consumers
{
    public class OrderCompleteConsumer : IConsumer<OrderComplete>
    {
        private readonly WorldDbContext _dbContext;
        public OrderCompleteConsumer(
            WorldDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }
        public async Task Consume(ConsumeContext<OrderComplete> context)
        {
            var message = context.Message;
            // delay 1s
            await Task.Delay(1000);
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
