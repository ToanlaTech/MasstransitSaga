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
        private readonly IRequestClient<CreateTodo> _createTodoClient;
        private readonly IRequestClient<UpdateTodo> _updateTodoClient;
        private readonly IRequestClient<DeleteTodo> _deleteTodoClient;
        private readonly IRequestClient<GetTodo> _getTodoClient;

        public TodoController(
            IRequestClient<GetTodos> getTodosClient,
            IRequestClient<CreateTodo> createTodoClient,
            IRequestClient<UpdateTodo> updateTodoClient,
            IRequestClient<DeleteTodo> deleteTodoClient,
            IRequestClient<GetTodo> getTodoClient)
        {
            _getTodosClient = getTodosClient;
            _createTodoClient = createTodoClient;
            _updateTodoClient = updateTodoClient;
            _deleteTodoClient = deleteTodoClient;
            _getTodoClient = getTodoClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodo(int id)
        {
            // Gửi request đến GetTodoConsumer và nhận response
            var response = await _getTodoClient.GetResponse<GetTodoResponse>(new GetTodo { Id = id });

            // Trả về kết quả
            return Ok(response.Message);
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos([FromQuery] int limit = 10, [FromQuery] int skip = 0)
        {
            var response = await _getTodosClient.GetResponse<GetTodosResponse>(new GetTodos { Limit = limit, Skip = skip });
            return Ok(response.Message);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodo todo)
        {
            // Gửi request đến consumer và nhận response
            var response = await _createTodoClient.GetResponse<Todo>(todo);

            // Trả về kết quả cho client
            return Ok(response.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] UpdateTodo updateTodo)
        {
            // Gửi request tới UpdateTodoConsumer và nhận response
            updateTodo.Id = id;
            var response = await _updateTodoClient.GetResponse<UpdateTodoResponse>(updateTodo);

            // Trả về kết quả
            return Ok(response.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            // Gửi request tới DeleteTodoConsumer và nhận response
            var response = await _deleteTodoClient.GetResponse<DeleteTodoResponse>(new DeleteTodo { Id = id });

            // Trả về kết quả
            return Ok(response.Message);
        }

    }
}
