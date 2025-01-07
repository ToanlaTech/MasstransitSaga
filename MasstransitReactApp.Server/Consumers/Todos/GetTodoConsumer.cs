using System;
using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;
using Newtonsoft.Json;

namespace MasstransitReactApp.Server.Consumers.Todos;

public class GetTodoConsumer : IConsumer<GetTodo>
{
    private readonly HttpClient _httpClient;

    public GetTodoConsumer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Consume(ConsumeContext<GetTodo> context)
    {
        try
        {
            // Gọi API để lấy thông tin Todo
            var response = await _httpClient.GetStringAsync($"https://dummyjson.com/todos/{context.Message.Id}");

            // Parse JSON response thành object GetTodoResponse
            var todo = JsonConvert.DeserializeObject<GetTodoResponse>(response);

            // Trả về kết quả
            await context.RespondAsync(todo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw; // Throw exception để kích hoạt Retry hoặc DLQ
        }
    }
}