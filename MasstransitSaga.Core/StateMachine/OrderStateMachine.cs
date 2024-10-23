using MasstransitSaga.Core.Models;
using MassTransit;

namespace MasstransitSaga.Core.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<Order>
    {
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }
        public State Completed { get; private set; }
        public State Cancelled { get; private set; }

        public Event<OrderSubmit> OrderSubmitEvent { get; private set; }
        public Event<OrderAccept> OrderAcceptEvent { get; private set; }
        public Event<OrderComplete> OrderCompleteEvent { get; private set; }
        public Event<OrderCancel> OrderCancelEvent { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderSubmitEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderAcceptEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderCompleteEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderCancelEvent, x => x.CorrelateById(context => context.Message.OrderId));

            #region Flow
            Initially(
                When(OrderSubmitEvent)
                    .Then(context =>
                    {
                        context.Saga.ProductId = context.Data.ProductId;
                        context.Saga.SubmittedAt = DateTime.UtcNow;
                    })
                    .TransitionTo(Submitted)
                    .ThenAsync(async context =>
                    {
                        // Tùy chọn: Ghi nhận lại thông tin hoặc bỏ qua
                        await Console.Out.WriteLineAsync($"Order {context.Saga.CorrelationId} has already been submitted.");
                    })
            );

            During(Submitted,
                When(OrderAcceptEvent)
                    .Then(context =>
                    {
                        context.Saga.AcceptedAt = DateTime.UtcNow;
                    })
                    .TransitionTo(Accepted)
                    .ThenAsync(async context =>
                    {
                        // Tùy chọn: Ghi nhận lại thông tin hoặc bỏ qua
                        await Console.Out.WriteLineAsync($"Order {context.Saga.CorrelationId} has already been accepted.");
                    })
            );

            DuringAny(
                When(OrderCancelEvent)
                .Then(context =>
                {
                    context.Saga.CancelledAt = DateTime.UtcNow;
                })
                .TransitionTo(Cancelled)
                .ThenAsync(async context =>
                {
                    // Tùy chọn: Ghi nhận lại thông tin hoặc bỏ qua
                    await Console.Out.WriteLineAsync($"Order {context.Saga.CorrelationId} has already been cancelled.");
                })

            );

            During(Accepted,
                When(OrderCompleteEvent)
                    .Then(context =>
                    {
                        context.Saga.CompletedAt = DateTime.UtcNow;
                    })
                    .TransitionTo(Completed)
                    .ThenAsync(async context =>
                    {
                        // Tùy chọn: Ghi nhận lại thông tin hoặc bỏ qua
                        await Console.Out.WriteLineAsync($"Order {context.Saga.CorrelationId} has already been completed.");
                    })
            );

            #endregion
        }
    }

    public record OrderAccept
    {
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public record OrderComplete
    {
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public record OrderCancel
    {
        public Guid OrderId { get; set; }
    }

    public record OrderSubmit(Guid OrderId, int ProductId, int Quantity);
    public record OrderSuccess(Guid OrderId, int ProductId, int Quantity);

    public class OrderRejected
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; }
    }

    public record OrderResponse(Guid OrderId, string Status, string Message);
}
