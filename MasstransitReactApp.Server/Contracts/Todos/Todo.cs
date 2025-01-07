using System;
using Newtonsoft.Json;

namespace MasstransitReactApp.Server.Contracts.Todos;

public class Todo
{
    public int Id { get; set; }
    [JsonProperty("todo")]
    public string TodoDescription { get; set; }
    public bool Completed { get; set; }
    public int UserId { get; set; }
}
