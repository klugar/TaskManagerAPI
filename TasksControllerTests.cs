using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagerAPI.Common;
using TaskManagerAPI.Controllers;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;
using TaskManagerAPI.Repositories;

namespace TaskManagerAPI.Tests;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepo;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockRepo = new Mock<ITaskRepository>();
        _controller = new TasksController(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1" },
            new() { Id = 2, Title = "Task 2" }
        };
        _mockRepo.Setup(r => r.GetAllAsync(default))
                 .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetAll(new TaskQueryDto(), default);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<TaskResponseDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count());
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(99, default))
                 .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.GetById(99, default);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedTask()
    {
        // Arrange
        var dto = new CreateTaskDto("New Task", "Desc", Priority.High, null, null);
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<TaskItem>(), default))
                 .ReturnsAsync((TaskItem t, CancellationToken _) =>
                     t with { Id = 1 });

        // Act
        var result = await _controller.Create(dto, default);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<TaskResponseDto>>(created.Value);
        Assert.True(response.Success);
        Assert.Equal("New Task", response.Data!.Title);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepo.Setup(r => r.DeleteAsync(99, default))
                 .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(99, default);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenTaskExists()
    {
        // Arrange
        _mockRepo.Setup(r => r.DeleteAsync(1, default))
                 .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, default);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse>(ok.Value);
        Assert.True(response.Success);
    }
}