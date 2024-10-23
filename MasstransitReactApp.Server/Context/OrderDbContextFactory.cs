using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MasstransitReactApp.Server.Context
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            // Sử dụng cấu hình từ appsettings.json hoặc các tệp cấu hình khác
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<OrderDbContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Sử dụng SQL Server cho DbContext của bạn
            builder.UseNpgsql(connectionString);

            return new OrderDbContext(builder.Options);
        }
    }
}
