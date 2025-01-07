using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;
using Newtonsoft.Json;

namespace MasstransitReactApp.Server.Consumers.Todos;

public class UpdateTodoConsumer : IConsumer<UpdateTodo>
{
    private readonly HttpClient _httpClient;

    public UpdateTodoConsumer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Consume(ConsumeContext<UpdateTodo> context)
    {
        // Gửi PUT request tới API
        var response = await _httpClient.PutAsJsonAsync($"https://dummyjson.com/todos/{context.Message.Id}", new
        {
            todo = context.Message.Todo,
            completed = context.Message.Completed
        });

        // Parse JSON response thành object UpdateTodoResponse
        var updatedTodo = JsonConvert.DeserializeObject<UpdateTodoResponse>(await response.Content.ReadAsStringAsync());

        // Trả về kết quả
        await context.RespondAsync(updatedTodo);
    }
}
