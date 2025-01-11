using System;

namespace MasstransitReactApp.Server.Contracts.Todos;

public class GetTodosResponse
{
    public List<Todo> Todos { get; set; }
    public int Total { get; set; }
    public int Skip { get; set; }
    public int Limit { get; set; }
}
