using MassTransit.EntityFrameworkCoreIntegration;
using MasstransitReactApp.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace MasstransitReactApp.Server.Context
{
    public class OrderDbContext : SagaDbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OrderStateMap(); }
        }
    }
}
