using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;
using Newtonsoft.Json;

namespace MasstransitReactApp.Server.Consumers.Todos;

public class GetTodosConsumer : IConsumer<GetTodos>
{
    private readonly HttpClient _httpClient;

    public GetTodosConsumer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Consume(ConsumeContext<GetTodos> context)
    {
        var response = await _httpClient.GetStringAsync($"https://dummyjson.com/todos?limit={context.Message.Limit}&skip={context.Message.Skip}");
        // Parse JSON thành object GetTodosResponse
        var todosResponse = JsonConvert.DeserializeObject<GetTodosResponse>(response);
        await context.RespondAsync(todosResponse);
    }
}
