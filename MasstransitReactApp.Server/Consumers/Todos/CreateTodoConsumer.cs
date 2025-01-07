using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;
using Newtonsoft.Json;

namespace MasstransitReactApp.Server.Consumers.Todos;

public class CreateTodoConsumer : IConsumer<CreateTodo>
{
    private readonly HttpClient _httpClient;

    public CreateTodoConsumer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Consume(ConsumeContext<CreateTodo> context)
    {
        // Gửi request POST đến API
        var response = await _httpClient.PostAsJsonAsync("https://dummyjson.com/todos/add", new
        {
            todo = context.Message.Todo,
            completed = context.Message.Completed,
            userId = context.Message.UserId
        });

        // Parse response từ API
        var createdTodo = await response.Content.ReadAsStringAsync();
        var todo = JsonConvert.DeserializeObject<Todo>(createdTodo);

        // Trả về kết quả đã parse
        await context.RespondAsync(todo);
    }
}
