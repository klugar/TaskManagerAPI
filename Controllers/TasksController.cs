using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Common;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;
using TaskManagerAPI.Repositories;


namespace TaskManagerAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _repo;

    public TasksController(ITaskRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<IEnumerable<TaskResponseDto>>>> GetAll(
    [FromQuery] TaskQueryDto query,
    CancellationToken ct)
    {
        if (query.PageNumber <= 0)
            return BadRequest(ApiResponse.Fail("Page number must be greater than zero."));

        if (query.PageSize <= 0 || query.PageSize > 100)
            return BadRequest(ApiResponse.Fail("Page size must be between 1 and 100."));

        var (items, totalCount) = await _repo.GetFilteredAsync(query, ct);

        return Ok(PagedResponse<IEnumerable<TaskResponseDto>>.Ok(
            items.Select(ToDto),
            query.PageNumber,
            query.PageSize,
            totalCount));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> GetById(
        int id, CancellationToken ct)
    {
        if (id <= 0)
            return BadRequest(ApiResponse<TaskResponseDto>.Fail("Id must be greater than zero."));

        var task = await _repo.GetByIdAsync(id, ct);
        if (task is null)
            return NotFound(ApiResponse<TaskResponseDto>.Fail($"Task {id} not found."));

        return Ok(ApiResponse<TaskResponseDto>.Ok(ToDto(task)));
    }

    [HttpGet("category/{categoryId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetByCategory(
        int categoryId, CancellationToken ct)
    {
        if (categoryId <= 0)
            return BadRequest(ApiResponse<IEnumerable<TaskResponseDto>>.Fail("CategoryId must be greater than zero."));

        var tasks = await _repo.GetByCategoryAsync(categoryId, ct);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks.Select(ToDto)));
    }

    [HttpGet("status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetByStatus(
        TodoStatus status, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(TodoStatus), status))
            return BadRequest(ApiResponse<IEnumerable<TaskResponseDto>>.Fail($"Invalid status value: {status}."));

        var tasks = await _repo.GetByStatusAsync(status, ct);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks.Select(ToDto)));
    }

    [HttpGet("overdue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetOverdue(
        CancellationToken ct)
    {
        var tasks = await _repo.GetOverdueAsync(ct);
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks.Select(ToDto)));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> Create(
    [FromBody] CreateTaskDto dto, CancellationToken ct)
    {
        // 409 — check for duplicate title
        var existing = await _repo.GetAllAsync(ct);
        if (existing.Any(t => t.Title.Equals(dto.Title, StringComparison.OrdinalIgnoreCase)))
            return Conflict(ApiResponse<TaskResponseDto>.Fail($"A task with title '{dto.Title}' already exists."));

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            CategoryId = dto.CategoryId
        };

        var created = await _repo.CreateAsync(task, ct);
        return CreatedAtAction(nameof(GetById),
            new { id = created.Id },
            ApiResponse<TaskResponseDto>.Ok(ToDto(created), "Task created successfully."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> Update(
    int id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
    {
        if (id <= 0)
            return BadRequest(ApiResponse<TaskResponseDto>.Fail("Id must be greater than zero."));

        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            return NotFound(ApiResponse<TaskResponseDto>.Fail($"Task {id} not found."));

        var updated = existing with
        {
            Title = dto.Title?.Trim() ?? existing.Title,
            Description = dto.Description?.Trim() ?? existing.Description,
            Status = dto.Status ?? existing.Status,
            Priority = dto.Priority ?? existing.Priority,
            DueDate = dto.DueDate ?? existing.DueDate,
            CategoryId = dto.CategoryId ?? existing.CategoryId
        };

        var saved = await _repo.UpdateAsync(updated, ct);
        return Ok(ApiResponse<TaskResponseDto>.Ok(ToDto(saved), "Task updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken ct)
    {
        if (id <= 0)
            return BadRequest(ApiResponse.Fail("Id must be greater than zero."));

        var deleted = await _repo.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound(ApiResponse.Fail($"Task {id} not found."));

        return Ok(ApiResponse.Ok("Task deleted successfully."));
    }

    private static TaskResponseDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description,
        t.Status.ToString(), t.Priority.ToString(),
        t.DueDate, t.CreatedAt, t.Category?.Name);
}