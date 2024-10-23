using MasstransitSaga.Core.Context;
using MasstransitSaga.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace MasstransitReactApp.Server.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly OrderDbContext _dbContext;
        public ProductController(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // create product range
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateProductRange([FromBody] List<Product> products)
        {
            await _dbContext.Products.AddRangeAsync(products);
            await _dbContext.SaveChangesAsync();
            return Ok("Product range created successfully!");
        }
    }
}
