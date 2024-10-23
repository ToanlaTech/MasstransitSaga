using MassTransit;

namespace MasstransitReactApp.Server.Models
{
    public class Order : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string? CurrentState { get; set; }
        public int ProductId { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
