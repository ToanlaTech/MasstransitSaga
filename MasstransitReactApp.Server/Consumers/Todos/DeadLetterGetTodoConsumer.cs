using System;
using System.Text.Json;
using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;

namespace MasstransitReactApp.Server.Consumers.Todos;

public class DeadLetterGetTodoConsumer : IConsumer<Fault<GetTodo>>
{
    public async Task Consume(ConsumeContext<Fault<GetTodo>> context)
    {
        // Log thông tin message lỗi
        Console.WriteLine("Message moved to Dead Letter Queue:");
        Console.WriteLine($"Original Message: {JsonSerializer.Serialize(context.Message.Message)}");
        Console.WriteLine($"Exception: {context.Message.Exceptions.FirstOrDefault()?.Message}");

        // Bạn có thể xử lý message lỗi tại đây (ví dụ: gửi thông báo, lưu database, v.v.)
        await Task.CompletedTask;
    }
}
