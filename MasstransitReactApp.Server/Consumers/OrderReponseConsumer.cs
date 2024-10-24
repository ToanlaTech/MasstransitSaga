using MassTransit;
using MasstransitReactApp.Server.SignalRHubs;
using MasstransitSaga.Core.StateMachine;
using Microsoft.AspNetCore.SignalR;

namespace MasstransitReactApp.Server.Consumers;

public class OrderReponseConsumer : IConsumer<OrderResponse>
{
    private readonly IHubContext<OrderStatusHub> _hubContext;
    public OrderReponseConsumer(IHubContext<OrderStatusHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<OrderResponse> context)
    {
        var message = context.Message;
        // await Task.Delay(1000);
        await Console.Out.WriteLineAsync("###############################################################################");
        await Console.Out.WriteLineAsync($"Order {message.OrderId} is {message.Status} with message: {message.Message}");
        await Console.Out.WriteLineAsync("###############################################################################");
        await _hubContext.Clients.Group(message.OrderId.ToString())
                    .SendAsync(message.Status, new
                    {
                        orderid = message.OrderId,
                        message = message.Message
                    });
    }
}
