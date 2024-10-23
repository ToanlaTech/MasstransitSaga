using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasstransitSaga.Core.Models
{
    public class OrderStateMap : SagaClassMap<Order>
    {
        protected override void Configure(EntityTypeBuilder<Order> entity, ModelBuilder model)
        {
            entity.ToTable("Order"); // Tên bảng là "OrderState"
            entity.HasKey(x => x.CorrelationId); // Đặt khóa chính
            entity.Property(x => x.ProductId);
            entity.Property(x => x.CurrentState);
            entity.Property(x => x.SubmittedAt);
            entity.Property(x => x.AcceptedAt);
            entity.Property(x => x.CompletedAt);
        }
    }
}
