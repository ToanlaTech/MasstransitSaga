using System;
using System.Text.Json;
using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;

namespace MasstransitReactApp.Server.Consumers.Todos;

public class TodoErrorConsumer : IConsumer<GetToDoError>
{
    public async Task Consume(ConsumeContext<GetToDoError> context)
    {
        // Log thông tin message lỗi

        Console.WriteLine("Message moved to Dead Letter Queue:");
        Console.WriteLine($"Class: {context.Message.Class}");
        Console.WriteLine($"Original Message: {context.Message.Message}");
        Console.WriteLine($"Exception: {context.Message.ExceptionMessage}");

        // Bạn có thể xử lý message lỗi tại đây (ví dụ: gửi thông báo, lưu database, v.v.)
        await Task.CompletedTask;
    }
}
