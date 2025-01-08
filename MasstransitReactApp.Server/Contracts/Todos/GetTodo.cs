using System;

namespace MasstransitReactApp.Server.Contracts.Todos;

public class GetTodo
{
    public int Id { get; set; }
}

public class GetToDoError
{
    public string Class { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
}
