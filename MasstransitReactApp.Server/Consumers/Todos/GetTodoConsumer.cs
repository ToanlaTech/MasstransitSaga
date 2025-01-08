using System;
using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;
using Newtonsoft.Json;

namespace MasstransitReactApp.Server.Consumers.Todos;

public class GetTodoConsumer : ExceptionTodoConsumer<GetTodo>, IConsumer<GetTodo>
{
    private readonly HttpClient _httpClient;

    public GetTodoConsumer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Consume(ConsumeContext<GetTodo> context)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var response = await _httpClient.GetAsync($"https://jsonplaceholder.typicode.com/todos/{context.Message.Id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var todo = JsonConvert.DeserializeObject<GetTodoResponse>(content);

            await context.RespondAsync(todo);
        }, context);
    }
}