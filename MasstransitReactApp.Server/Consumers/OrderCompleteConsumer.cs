using MassTransit;
using MasstransitReactApp.Server.Context;
using MasstransitReactApp.Server.SignalRHubs;
using MasstransitReactApp.Server.StateMachine;
using Microsoft.AspNetCore.SignalR;

namespace MasstransitReactApp.Server.Consumers
{
    public class OrderCompleteConsumer : IConsumer<OrderComplete>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IHubContext<OrderStatusHub> _hubContext;
        public OrderCompleteConsumer(
            OrderDbContext dbContext,
            IHubContext<OrderStatusHub> hubContext
            )
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
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
                _hubContext.Clients.Group(message.OrderId.ToString())
                    .SendAsync("OrderCompleted", new
                    {
                        orderid = message.OrderId,
                        message = "Product is completed."
                    })
            );
        }
    }
}
