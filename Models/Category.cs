namespace TaskManagerAPI.Models;

public record Category
{
    public int    Id    { get; init; }
    public string Name  { get; init; } = string.Empty;
    public string Color { get; init; } = "#6366f1";
    public ICollection<TaskItem> Tasks { get; init; } = new List<TaskItem>();
}