using System;

namespace MasstransitReactApp.Server.Contracts.Todos;

public class GetTodos
{
    public int Limit { get; set; }
    public int Skip { get; set; }
}
