using MassTransit;
using MasstransitReactApp.Server.Context;
using MasstransitReactApp.Server.SignalRHubs;
using MasstransitReactApp.Server.StateMachine;
using Microsoft.AspNetCore.SignalR;

namespace MasstransitReactApp.Server.Consumers
{
    public class OrderAcceptConsumer : IConsumer<OrderAccept>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IHubContext<OrderStatusHub> _hubContext;
        public OrderAcceptConsumer(
            OrderDbContext dbContext,
            IHubContext<OrderStatusHub> hubContext
        )
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
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
                        orderid = message.OrderId,
                        message = "Product is out of stock."
                    }),
                    _hubContext.Clients.Group(message.OrderId.ToString())
                    .SendAsync("OrderRejected", new
                    {
                        orderid = message.OrderId,
                        message = "Product is out of stock."
                    })
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
