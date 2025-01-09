using MasstransitSaga.Core.Models;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace MasstransitSaga.Core.Context
{
    public class WorldDbContext : SagaDbContext
    {
        public WorldDbContext(DbContextOptions<WorldDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Countrylanguage> CountryLanguages { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OrderStateMap(); }
        }
    }
}
