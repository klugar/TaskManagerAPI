using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Common;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;
using TaskManagerAPI.Repositories;

namespace TaskManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _repo;

    public TasksController(ITaskRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetAll(
        CancellationToken ct)
    {
        var tasks = await _repo.GetAllAsync(ct);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks.Select(ToDto)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> GetById(
        int id, CancellationToken ct)
    {
        var task = await _repo.GetByIdAsync(id, ct);
        if (task is null)
            return NotFound(ApiResponse<TaskResponseDto>.Fail($"Task {id} not found."));

        return Ok(ApiResponse<TaskResponseDto>.Ok(ToDto(task)));
    }

    [HttpGet("category/{categoryId:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetByCategory(
        int categoryId, CancellationToken ct)
    {
        var tasks = await _repo.GetByCategoryAsync(categoryId, ct);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks.Select(ToDto)));
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetByStatus(
        TodoStatus status, CancellationToken ct)
    {
        var tasks = await _repo.GetByStatusAsync(status, ct);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks.Select(ToDto)));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetOverdue(
        CancellationToken ct)
    {
        var tasks = await _repo.GetOverdueAsync(ct);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks.Select(ToDto)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> Create(
        [FromBody] CreateTaskDto dto, CancellationToken ct)
    {
        var task = new TaskItem
        {
            Title       = dto.Title,
            Description = dto.Description,
            Priority    = dto.Priority,
            DueDate     = dto.DueDate,
            CategoryId  = dto.CategoryId
        };

        var created = await _repo.CreateAsync(task, ct);
        return CreatedAtAction(nameof(GetById),
            new { id = created.Id },
            ApiResponse<TaskResponseDto>.Ok(ToDto(created), "Task created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> Update(
        int id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            return NotFound(ApiResponse<TaskResponseDto>.Fail($"Task {id} not found."));

        var updated = existing with
        {
            Title       = dto.Title       ?? existing.Title,
            Description = dto.Description ?? existing.Description,
            Status      = dto.Status      ?? existing.Status,
            Priority    = dto.Priority    ?? existing.Priority,
            DueDate     = dto.DueDate     ?? existing.DueDate,
            CategoryId  = dto.CategoryId  ?? existing.CategoryId
        };

        var saved = await _repo.UpdateAsync(updated, ct);
        return Ok(ApiResponse<TaskResponseDto>.Ok(ToDto(saved), "Task updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken ct)
    {
        var deleted = await _repo.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound(ApiResponse.Fail($"Task {id} not found."));

        return Ok(ApiResponse.Ok("Task deleted successfully."));
    }

    // Private helper — converts TaskItem to TaskResponseDto
    private static TaskResponseDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description,
        t.Status.ToString(), t.Priority.ToString(),
        t.DueDate, t.CreatedAt, t.Category?.Name);
}