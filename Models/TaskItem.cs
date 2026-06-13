namespace TaskManagerAPI.Models;

public enum TodoStatus { Todo, InProgress, Done }  // renamed from TaskStatus
public enum Priority   { Low, Medium, High }

public record TaskItem
{
    public int        Id          { get; init; }
    public string     Title       { get; init; } = string.Empty;
    public string?    Description { get; init; }
    public TodoStatus Status      { get; init; } = TodoStatus.Todo;  // updated
    public Priority   Priority    { get; init; } = Priority.Medium;
    public DateTime?  DueDate     { get; init; }
    public DateTime   CreatedAt   { get; init; } = DateTime.UtcNow;
    public int?       CategoryId  { get; init; }
    public Category?  Category    { get; init; }
}