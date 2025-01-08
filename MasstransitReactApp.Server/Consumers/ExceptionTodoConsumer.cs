
using MassTransit;
using MasstransitReactApp.Server.Contracts.Todos;
using Newtonsoft.Json;

public class ExceptionTodoConsumer<T> where T : class
{
    public async Task ExecuteWithRetryAsync(Func<Task> action, ConsumeContext<T> context)
    {
        try
        {
            await action();
        }
        // catch Http Reponse Code 500
        catch (HttpRequestException ex) when (context.GetRetryAttempt() < 3)
        {
            Console.WriteLine($"Error at HttpRequestException: {ex.Message}");
            throw; // Throw exception để kích hoạt Retry hoặc DLQ
        }
        catch (TimeoutException ex) when (context.GetRetryAttempt() < 3)
        {
            Console.WriteLine($"Error at TimeoutException: {ex.Message}");
            throw; // Throw exception để kích hoạt Retry hoặc DLQ
        }
        catch (OperationCanceledException ex) when (context.GetRetryAttempt() < 3)
        {
            Console.WriteLine($"Error at OperationCanceledException: {ex.Message}");
            throw; // Throw exception để kích hoạt Retry hoặc DLQ
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error at Exception: {ex.Message}");

            await context.Publish(new GetToDoError
            {
                Class = typeof(T).Name,
                Message = JsonConvert.SerializeObject(context.Message),
                ExceptionMessage = JsonConvert.SerializeObject(new
                {
                    ex.Message,
                    ex.StackTrace,
                    InnerExceptionMessage = ex.InnerException?.Message // Nếu có lỗi lồng bên trong
                })
            });

            throw; // Throw exception để kích hoạt Retry hoặc DLQ
        }
    }
}