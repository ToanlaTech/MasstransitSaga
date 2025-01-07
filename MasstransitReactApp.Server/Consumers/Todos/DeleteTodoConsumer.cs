using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;
using Newtonsoft.Json;

namespace MasstransitReactApp.Server.Consumers.Todos;

public class DeleteTodoConsumer : IConsumer<DeleteTodo>
{
    private readonly HttpClient _httpClient;

    public DeleteTodoConsumer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Consume(ConsumeContext<DeleteTodo> context)
    {
        // Gửi DELETE request tới API
        var response = await _httpClient.DeleteAsync($"https://dummyjson.com/todos/{context.Message.Id}");

        // Parse JSON response thành object DeleteTodoResponse
        var deletedTodo = JsonConvert.DeserializeObject<DeleteTodoResponse>(await response.Content.ReadAsStringAsync());

        // Trả về kết quả
        await context.RespondAsync(deletedTodo);
    }
}
