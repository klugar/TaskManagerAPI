using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Common;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;
using TaskManagerAPI.Repositories;

namespace TaskManagerAPI.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/tasks")]
public class TasksV2Controller : ControllerBase
{
    private readonly ITaskRepository _repo;

    public TasksV2Controller(ITaskRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] TaskQueryDto query, CancellationToken ct)
    {
        var (items, totalCount) = await _repo.GetFilteredAsync(query, ct);

        // V2 returns extra summary data
        var tasks = items.Select(ToDto).ToList();
        var summary = new
        {
            TotalTasks = totalCount,
            TodoCount = tasks.Count(t => t.Status == "Todo"),
            InProgressCount = tasks.Count(t => t.Status == "InProgress"),
            DoneCount = tasks.Count(t => t.Status == "Done"),
            OverdueCount = tasks.Count(t => t.DueDate.HasValue
                                && t.DueDate < DateTime.UtcNow
                                && t.Status != "Done")
        };

        return Ok(ApiResponse<object>.Ok(new { Tasks = tasks, Summary = summary }));
    }

    [HttpGet("{id:int}")]
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

    private static TaskResponseDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description,
        t.Status.ToString(), t.Priority.ToString(),
        t.DueDate, t.CreatedAt, t.Category?.Name);
}