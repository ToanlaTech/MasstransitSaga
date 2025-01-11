using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MasstransitReactApp.Server.Controllers
{
    [Route("api/todos")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly IRequestClient<GetTodos> _getTodosClient;

        public TodoController(IRequestClient<GetTodos> getTodosClient)
        {
            _getTodosClient = getTodosClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos([FromQuery] int limit = 10, [FromQuery] int skip = 0)
        {
            var response = await _getTodosClient.GetResponse<GetTodosResponse>(new GetTodos { Limit = limit, Skip = skip });
            return Ok(response.Message);
        }
    }
}
