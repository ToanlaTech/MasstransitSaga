using MasstransitSaga.Core.StateMachine;
using MassTransit;
using MasstransitReactApp.Server.SignalRHubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MasstransitReactApp.Server.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHubContext<OrderStatusHub> _hubContext;
        public OrdersController(
            IPublishEndpoint publishEndpoint,
            IHubContext<OrderStatusHub> hubContext
            )
        {
            _publishEndpoint = publishEndpoint;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("submit")]
        public async Task<IActionResult> SubmitOrder([FromBody] OrderSubmit order)
        {
            await Task.WhenAll(
                _publishEndpoint.Publish(order),
                _hubContext.Clients.Group(order.OrderId.ToString())
                    .SendAsync("OrderSubmitted", new
                    {
                        orderid = order.OrderId,
                        message = "Order is submitted."
                    })
            );
            return Ok("Order submitted successfully!");
        }

        [HttpPost]
        [Route("accept")]
        public async Task<IActionResult> AcceptOrder([FromBody] OrderAccept order)
        {
            // Publish the message to the queue
            await _publishEndpoint.Publish(order);
            return Ok("Order accepted successfully!");
        }

        [HttpPost]
        [Route("complete")]
        public async Task<IActionResult> CompleteOrder([FromBody] OrderComplete order)
        {
            // Publish the message to the queue
            await _publishEndpoint.Publish(order);
            return Ok("Order completed successfully!");
        }

        [HttpPost]
        [Route("response")]
        public async Task<IActionResult> ResponseOrder([FromBody] OrderResponse order)
        {
            // Publish the message to the queue
            await _publishEndpoint.Publish(order);
            return Ok("Order response sent successfully!");
        }
    }
}
