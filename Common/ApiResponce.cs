namespace TaskManagerAPI.Common;

public record ApiResponse<T>
{
    public bool    Success { get; init; }
    public T?      Data    { get; init; }
    public string? Error   { get; init; }
    public string? Message { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string error) =>
        new() { Success = false, Error = error };
}

public record ApiResponse
{
    public bool    Success { get; init; }
    public string? Error   { get; init; }
    public string? Message { get; init; }

    public static ApiResponse Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ApiResponse Fail(string error) =>
        new() { Success = false, Error = error };
}


public record PagedResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public static PagedResponse<T> Ok(T data, int pageNumber, int pageSize, int totalCount) =>
        new()
        {
            Success = true,
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
}