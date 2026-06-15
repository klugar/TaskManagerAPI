using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Repositories;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> GetByStatusAsync(TodoStatus status, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> GetOverdueAsync(CancellationToken ct = default);
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken ct = default);

    // New — filtered, sorted, paged
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetFilteredAsync(
        TaskQueryDto query, CancellationToken ct = default);
}