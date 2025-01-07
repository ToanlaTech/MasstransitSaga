using System;

namespace MasstransitReactApp.Server.Contracts.Todos;

public class UpdateTodoResponse
{
    public int Id { get; set; }
    public string Todo { get; set; }
    public bool Completed { get; set; }
    public int UserId { get; set; }
}
