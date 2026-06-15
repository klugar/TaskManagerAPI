namespace TaskManagerAPI.DTOs;

public record TaskQueryDto
{
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public int? CategoryId { get; init; }
    public string? Search { get; init; }  // search by title
    public string? Sort { get; init; }  // title, dueDate, createdAt, priority
    public string? Order { get; init; }  // asc, desc
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}