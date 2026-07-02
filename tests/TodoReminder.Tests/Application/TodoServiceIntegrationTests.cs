using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoReminder.App.Services;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Database;
using TodoReminder.Infrastructure.Repositories.Implementations;

namespace TodoReminder.Tests.Application;

public class TodoServiceIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly TodoService _service;

    public TodoServiceIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        var repository = new TodoRepository(_context);
        _service = new TodoService(repository);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Fact]
    public async Task CreateTodo_ShouldReturnDtoWithId()
    {
        var dto = await _service.CreateTodoAsync("测试事项", "备注", new DateOnly(2026, 6, 25), new TimeOnly(10, 30));

        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("测试事项", dto.Title);
        Assert.Equal("备注", dto.Note);
        Assert.Equal(new DateOnly(2026, 6, 25), dto.RecordDate);
        Assert.Equal(new TimeOnly(10, 30), dto.DueTime);
        Assert.True(dto.ReminderEnabled);
    }

    [Fact]
    public async Task GetTodosByDate_ShouldReturnCreatedItems()
    {
        await _service.CreateTodoAsync("事项A", null, new DateOnly(2026, 6, 25), null);
        await _service.CreateTodoAsync("事项B", null, new DateOnly(2026, 6, 25), new TimeOnly(14, 0));

        var items = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));

        Assert.Equal(2, items.Count());
        Assert.Contains(items, i => i.Title == "事项A");
        Assert.Contains(items, i => i.Title == "事项B");
    }

    [Fact]
    public async Task GetTodosByDate_ShouldNotReturnOtherDates()
    {
        await _service.CreateTodoAsync("今天事项", null, new DateOnly(2026, 6, 25), null);
        await _service.CreateTodoAsync("昨天事项", null, new DateOnly(2026, 6, 24), null);

        var todayItems = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));

        Assert.Single(todayItems);
        Assert.Equal("今天事项", todayItems.First().Title);
    }

    [Fact]
    public async Task UpdateTodo_ShouldChangeFields()
    {
        var dto = await _service.CreateTodoAsync("原始标题", "原始备注", new DateOnly(2026, 6, 25), null);

        var updated = await _service.UpdateTodoAsync(dto.Id, "新标题", "新备注", new TimeOnly(9, 0));

        Assert.Equal("新标题", updated.Title);
        Assert.Equal("新备注", updated.Note);
        Assert.Equal(new TimeOnly(9, 0), updated.DueTime);
    }

    [Fact]
    public async Task CompleteTodo_ShouldSetStatusAndTimestamp()
    {
        var dto = await _service.CreateTodoAsync("可完成事项", null, new DateOnly(2026, 6, 25), null);

        await _service.CompleteTodoAsync(dto.Id);

        var items = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));
        Assert.Equal(TodoStatus.Completed, items.First().Status);
    }

    [Fact]
    public async Task AbandonTodo_ShouldSetStatus()
    {
        var dto = await _service.CreateTodoAsync("可废弃事项", null, new DateOnly(2026, 6, 25), null);

        await _service.AbandonTodoAsync(dto.Id);

        var items = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));
        Assert.Equal(TodoStatus.Abandoned, items.First().Status);
    }

    [Fact]
    public async Task RestoreTodo_ShouldResetToPending()
    {
        var dto = await _service.CreateTodoAsync("可恢复事项", null, new DateOnly(2026, 6, 25), null);
        await _service.CompleteTodoAsync(dto.Id);

        await _service.RestoreTodoAsync(dto.Id);

        var items = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));
        Assert.Equal(TodoStatus.Pending, items.First().Status);
    }

    [Fact]
    public async Task DeleteTodo_ShouldRemoveFromList()
    {
        var dto = await _service.CreateTodoAsync("待删除事项", null, new DateOnly(2026, 6, 25), null);

        await _service.DeleteTodoAsync(dto.Id);

        var items = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));
        Assert.Empty(items);
    }

    [Fact]
    public async Task CreateTodo_WithoutDueTime_ShouldDisableReminder()
    {
        var dto = await _service.CreateTodoAsync("无提醒事项", null, new DateOnly(2026, 6, 25), null);

        Assert.Null(dto.DueTime);
        Assert.False(dto.ReminderEnabled);
    }

    [Fact]
    public async Task MultipleStatusChanges_ShouldWorkCorrectly()
    {
        var dto = await _service.CreateTodoAsync("状态测试", null, new DateOnly(2026, 6, 25), null);

        await _service.CompleteTodoAsync(dto.Id);
        var items1 = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));
        Assert.Equal(TodoStatus.Completed, items1.First().Status);

        await _service.RestoreTodoAsync(dto.Id);
        var items2 = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));
        Assert.Equal(TodoStatus.Pending, items2.First().Status);

        await _service.AbandonTodoAsync(dto.Id);
        var items3 = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));
        Assert.Equal(TodoStatus.Abandoned, items3.First().Status);

        await _service.RestoreTodoAsync(dto.Id);
        var items4 = await _service.GetTodosByDateAsync(new DateOnly(2026, 6, 25));
        Assert.Equal(TodoStatus.Pending, items4.First().Status);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnCompletedAndAbandonedRecords()
    {
        var dto1 = await _service.CreateTodoAsync("已完成事项", null, new DateOnly(2026, 6, 25), null);
        var dto2 = await _service.CreateTodoAsync("已废弃事项", null, new DateOnly(2026, 6, 25), null);
        var dto3 = await _service.CreateTodoAsync("待办事项", null, new DateOnly(2026, 6, 25), null);

        await _service.CompleteTodoAsync(dto1.Id);
        await _service.AbandonTodoAsync(dto2.Id);

        var history = await _service.GetHistoryAsync();

        Assert.Equal(2, history.Count);
        Assert.Contains(history, h => h.Title == "已完成事项" && h.Status == TodoStatus.Completed);
        Assert.Contains(history, h => h.Title == "已废弃事项" && h.Status == TodoStatus.Abandoned);

        // Verify StatusTimeDisplay does not throw
        foreach (var item in history)
        {
            var display = item.StatusTimeDisplay;
            var statusDisp = item.StatusDisplay;
            Assert.False(string.IsNullOrEmpty(display));
            Assert.False(string.IsNullOrEmpty(statusDisp));
        }
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldBeOrderedByCompletionTimeDesc()
    {
        var dto1 = await _service.CreateTodoAsync("先完成", null, new DateOnly(2026, 6, 25), null);
        var dto2 = await _service.CreateTodoAsync("后完成", null, new DateOnly(2026, 6, 25), null);

        await _service.CompleteTodoAsync(dto1.Id);
        await Task.Delay(10);
        await _service.CompleteTodoAsync(dto2.Id);

        var history = await _service.GetHistoryAsync();

        Assert.Equal(2, history.Count);
        Assert.Equal("后完成", history[0].Title);
        Assert.Equal("先完成", history[1].Title);
    }
}
