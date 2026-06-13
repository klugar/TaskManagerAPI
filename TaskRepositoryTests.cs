using Microsoft.EntityFrameworkCore;
using System;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using TaskManagerAPI.Repositories;

namespace TaskManagerAPI.Tests;

public class TaskRepositoryTests
{
    private AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTasks()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var repo = new TaskRepository(db);

        await repo.CreateAsync(new TaskItem { Title = "Task 1" });
        await repo.CreateAsync(new TaskItem { Title = "Task 2" });

        // Act
        var tasks = await repo.GetAllAsync();

        // Assert
        Assert.Equal(2, tasks.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectTask()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var repo = new TaskRepository(db);
        var created = await repo.CreateAsync(new TaskItem { Title = "My Task" });

        // Act
        var task = await repo.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(task);
        Assert.Equal("My Task", task.Title);
    }

    [Fact]
    public async Task CreateAsync_SavesTaskToDatabase()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var repo = new TaskRepository(db);

        // Act
        var task = await repo.CreateAsync(new TaskItem
        {
            Title = "New Task",
            Description = "Test description",
            Priority = Priority.High
        });

        // Assert
        Assert.True(task.Id > 0);
        Assert.Equal("New Task", task.Title);
        Assert.Equal(Priority.High, task.Priority);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTaskFromDatabase()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var repo = new TaskRepository(db);
        var task = await repo.CreateAsync(new TaskItem { Title = "To Delete" });

        // Act
        var result = await repo.DeleteAsync(task.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await repo.GetByIdAsync(task.Id));
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyMatchingTasks()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var repo = new TaskRepository(db);

        await repo.CreateAsync(new TaskItem { Title = "Todo Task", Status = TodoStatus.Todo });
        await repo.CreateAsync(new TaskItem { Title = "Done Task", Status = TodoStatus.Done });
        await repo.CreateAsync(new TaskItem { Title = "Todo Task2", Status = TodoStatus.Todo });

        // Act
        var todoTasks = await repo.GetByStatusAsync(TodoStatus.Todo);

        // Assert
        Assert.Equal(2, todoTasks.Count());
        Assert.All(todoTasks, t => Assert.Equal(TodoStatus.Todo, t.Status));
    }

    [Fact]
    public async Task GetOverdueAsync_ReturnsOnlyOverdueTasks()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var repo = new TaskRepository(db);

        await repo.CreateAsync(new TaskItem
        {
            Title = "Overdue Task",
            DueDate = DateTime.UtcNow.AddDays(-5),
            Status = TodoStatus.Todo
        });
        await repo.CreateAsync(new TaskItem
        {
            Title = "Future Task",
            DueDate = DateTime.UtcNow.AddDays(5),
            Status = TodoStatus.Todo
        });

        // Act
        var overdue = await repo.GetOverdueAsync();

        // Assert
        Assert.Single(overdue);
        Assert.Equal("Overdue Task", overdue.First().Title);
    }
}