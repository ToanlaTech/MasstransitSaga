using System;

namespace MasstransitReactApp.Server.Contracts.Todos;

public class DeleteTodoResponse
{
    public int Id { get; set; }
    public string Todo { get; set; }
    public bool Completed { get; set; }
    public int UserId { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedOn { get; set; } // ISO date format
}
