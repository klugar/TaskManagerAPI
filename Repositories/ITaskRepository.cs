using TaskManagerAPI.Models;

namespace TaskManagerAPI.Repositories;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> GetByStatusAsync(TodoStatus status, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> GetOverdueAsync(CancellationToken ct = default);
}