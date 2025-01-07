using System;

namespace MasstransitReactApp.Server.Contracts.Todos;

public class CreateTodo
{
    public string Todo { get; set; }
    public bool Completed { get; set; }
    public int UserId { get; set; }
}
