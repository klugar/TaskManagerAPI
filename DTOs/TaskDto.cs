namespace TaskManagerAPI.DTOs;

using TaskManagerAPI.Models;

public record CreateTaskDto(
    string    Title,
    string?   Description,
    Priority  Priority,
    DateTime? DueDate,
    int?      CategoryId
);

public record UpdateTaskDto(
    string?     Title,
    string?     Description,
    TodoStatus? Status,      // was TaskStatus
    Priority?   Priority,
    DateTime?   DueDate,
    int?        CategoryId
);

public record TaskResponseDto(
    int       Id,
    string    Title,
    string?   Description,
    string    Status,
    string    Priority,
    DateTime? DueDate,
    DateTime  CreatedAt,
    string?   CategoryName
);