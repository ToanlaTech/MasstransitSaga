using System;

namespace MasstransitReactApp.Server.Contracts.Todos;

public class UpdateTodo
{
    public int Id { get; set; }
    public string Todo { get; set; }
    public bool Completed { get; set; }
}
