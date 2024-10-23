using MassTransit;
using MasstransitReactApp.Server.Context;
using MasstransitReactApp.Server.SignalRHubs;
using MasstransitReactApp.Server.StateMachine;
using Microsoft.AspNetCore.SignalR;

namespace MasstransitReactApp.Server.Consumers
{
    public class OrderSubmitConsumer : IConsumer<OrderSubmit>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IHubContext<OrderStatusHub> _hubContext;
        public OrderSubmitConsumer(
            OrderDbContext dbContext,
            IHubContext<OrderStatusHub> hubContext
            )
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
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
                    _hubContext.Clients.Group(message.OrderId.ToString())
                    .SendAsync("OrderRejected", new
                    {
                        orderid = message.OrderId,
                        message = "Product not found."
                    })
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
                    _hubContext.Clients.Group(message.OrderId.ToString())
                    .SendAsync("OrderAccepted", new
                    {
                        orderid = message.OrderId,
                        message = "Product is accepted."
                    })
                );
            }
        }
    }
}
