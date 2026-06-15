using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
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

    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedAsync(
    int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Tasks.Include(t => t.Category);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetFilteredAsync(
    TaskQueryDto query, CancellationToken ct = default)
    {
        // Start with base query
        var q = _db.Tasks.Include(t => t.Category).AsQueryable();

        // Filter by status
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<TodoStatus>(query.Status, true, out var status))
            q = q.Where(t => t.Status == status);

        // Filter by priority
        if (!string.IsNullOrWhiteSpace(query.Priority) &&
            Enum.TryParse<Priority>(query.Priority, true, out var priority))
            q = q.Where(t => t.Priority == priority);

        // Filter by category
        if (query.CategoryId.HasValue)
            q = q.Where(t => t.CategoryId == query.CategoryId);

        // Search by title
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(t => t.Title.Contains(query.Search));

        // Get total before pagination
        var totalCount = await q.CountAsync(ct);

        // Sorting
        q = query.Sort?.ToLower() switch
        {
            "title" => query.Order == "desc"
                            ? q.OrderByDescending(t => t.Title)
                            : q.OrderBy(t => t.Title),
            "duedate" => query.Order == "desc"
                            ? q.OrderByDescending(t => t.DueDate)
                            : q.OrderBy(t => t.DueDate),
            "priority" => query.Order == "desc"
                            ? q.OrderByDescending(t => t.Priority)
                            : q.OrderBy(t => t.Priority),
            _ => q.OrderByDescending(t => t.CreatedAt) // default
        };

        // Pagination
        var items = await q
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

}