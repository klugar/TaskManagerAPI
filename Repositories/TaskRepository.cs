using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;


namespace TaskManagerAPI.Repositories;

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(AppDbContext db) : base(db) { }

    public override async Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken ct = default)
    => await _set.ToListAsync(ct);

    public async Task<IEnumerable<TaskItem>> GetByCategoryAsync(
        int categoryId, CancellationToken ct = default)
        => await _db.Tasks
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync(ct);

    public async Task<IEnumerable<TaskItem>> GetByStatusAsync(
    TodoStatus status, CancellationToken ct = default)
    => await _db.Tasks
        .Include(t => t.Category)
        .Where(t => t.Status == status)
        .ToListAsync(ct);

public async Task<IEnumerable<TaskItem>> GetOverdueAsync(CancellationToken ct = default)
    => await _db.Tasks
        .Include(t => t.Category)
        .Where(t => t.DueDate < DateTime.UtcNow && t.Status != TodoStatus.Done)
        .OrderBy(t => t.DueDate)
        .ToListAsync(ct);
}