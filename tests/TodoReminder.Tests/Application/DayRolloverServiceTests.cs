using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoReminder.App.Services;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Database;
using TodoReminder.Infrastructure.Repositories.Implementations;

namespace TodoReminder.Tests.Application;

public class DayRolloverServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly TodoRepository _repository;
    private readonly DayRolloverService _service;
    private readonly DateOnly _today;
    private readonly DateOnly _yesterday;

    public DayRolloverServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new TodoRepository(_context);
        _service = new DayRolloverService(_repository);
        _today = DateOnly.FromDateTime(DateTime.Now);
        _yesterday = _today.AddDays(-1);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    private async Task<TodoTask> CreateTask(string title)
    {
        var task = new TodoTask(title, null);
        await _repository.AddTaskAsync(task);
        return task;
    }

    private async Task<TodoDailyRecord> CreateRecord(Guid taskId, DateOnly date, TodoStatus status, bool reminder = false)
    {
        var record = new TodoDailyRecord(taskId, date)
        {
            Status = status,
            ReminderEnabled = reminder,
            DueTime = reminder ? new TimeOnly(9, 0) : null
        };
        await _repository.AddRecordAsync(record);
        return record;
    }

    [Fact]
    public async Task Pending_ShouldRollover()
    {
        var task = await CreateTask("测试");
        await CreateRecord(task.Id, _yesterday, TodoStatus.Pending);

        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Single(todayRecords);
        Assert.Equal(task.Id, todayRecords[0].TaskId);
        Assert.Equal(TodoStatus.Pending, todayRecords[0].Status);
        Assert.NotNull(todayRecords[0].InheritedFromRecordId);
    }

    [Fact]
    public async Task Completed_ShouldNotRollover()
    {
        var task = await CreateTask("已完成");
        await CreateRecord(task.Id, _yesterday, TodoStatus.Completed);

        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Empty(todayRecords);
    }

    [Fact]
    public async Task Abandoned_ShouldNotRollover()
    {
        var task = await CreateTask("已废弃");
        await CreateRecord(task.Id, _yesterday, TodoStatus.Abandoned);

        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Empty(todayRecords);
    }

    [Fact]
    public async Task DeletedTask_ShouldNotRollover()
    {
        var task = await CreateTask("已删除");
        task.DeletedAt = DateTime.UtcNow;
        await _repository.UpdateTaskAsync(task);
        await CreateRecord(task.Id, _yesterday, TodoStatus.Pending);

        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Empty(todayRecords);
    }

    [Fact]
    public async Task ExistingTodayRecord_ShouldNotDuplicate()
    {
        var task = await CreateTask("不重复");
        await CreateRecord(task.Id, _yesterday, TodoStatus.Pending);

        await _service.EnsureRolloverAsync();
        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Single(todayRecords);
    }

    [Fact]
    public async Task MixedStatus_ShouldOnlyRolloverPending()
    {
        var task1 = await CreateTask("待处理");
        var task2 = await CreateTask("已完成");
        var task3 = await CreateTask("已废弃");
        await CreateRecord(task1.Id, _yesterday, TodoStatus.Pending);
        await CreateRecord(task2.Id, _yesterday, TodoStatus.Completed);
        await CreateRecord(task3.Id, _yesterday, TodoStatus.Abandoned);

        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Single(todayRecords);
        Assert.Equal(task1.Id, todayRecords[0].TaskId);
    }

    [Fact]
    public async Task ReminderSettings_ShouldBePreserved()
    {
        var task = await CreateTask("带提醒");
        await CreateRecord(task.Id, _yesterday, TodoStatus.Pending, reminder: true);

        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.True(todayRecords[0].ReminderEnabled);
        Assert.Equal(new TimeOnly(9, 0), todayRecords[0].DueTime);
        Assert.Null(todayRecords[0].ReminderFiredAt);
    }

    [Fact]
    public async Task NoPendingBeforeToday_ShouldDoNothing()
    {
        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Empty(todayRecords);
    }

    [Fact]
    public async Task PendingFromTwoDaysAgo_ShouldRollover()
    {
        var twoDaysAgo = _today.AddDays(-2);
        var task = await CreateTask("两天前");
        await CreateRecord(task.Id, twoDaysAgo, TodoStatus.Pending);

        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Single(todayRecords);
        Assert.Equal(task.Id, todayRecords[0].TaskId);
    }

    [Fact]
    public async Task InheritedRecord_ShouldSetSource()
    {
        var task = await CreateTask("继承来源");
        var source = await CreateRecord(task.Id, _yesterday, TodoStatus.Pending);

        await _service.EnsureRolloverAsync();

        var todayRecords = await _repository.GetRecordsByDateAsync(_today);
        Assert.Equal(source.Id, todayRecords[0].InheritedFromRecordId);
    }
}
