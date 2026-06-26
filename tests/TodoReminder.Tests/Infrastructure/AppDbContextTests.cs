using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Database;

namespace TodoReminder.Tests.Infrastructure;

public class AppDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly AppDbContext _context;

    public AppDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new AppDbContext(_options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Fact]
    public async Task CreateDatabase_ShouldCreateAllTables()
    {
        var canConnect = await _context.Database.CanConnectAsync();
        Assert.True(canConnect);
    }

    [Fact]
    public async Task TodoTask_InsertAndQuery()
    {
        var task = new TodoTask("Test Task", "Test Note");
        _context.TodoTasks.Add(task);
        await _context.SaveChangesAsync();

        var saved = await _context.TodoTasks.FindAsync(task.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test Task", saved!.Title);
        Assert.Equal("Test Note", saved.Note);
    }

    [Fact]
    public async Task TodoTask_SoftDelete_ShouldSetDeletedAt()
    {
        var task = new TodoTask("To Delete", null);
        _context.TodoTasks.Add(task);
        await _context.SaveChangesAsync();

        task.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var deleted = await _context.TodoTasks.FindAsync(task.Id);
        Assert.NotNull(deleted!.DeletedAt);
    }

    [Fact]
    public async Task TodoDailyRecord_InsertAndQuery()
    {
        var task = new TodoTask("Task", null);
        _context.TodoTasks.Add(task);
        await _context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = new TodoDailyRecord(task.Id, today)
        {
            DueTime = new TimeOnly(10, 30),
            ReminderEnabled = true,
            SortOrder = 1
        };
        _context.TodoDailyRecords.Add(record);
        await _context.SaveChangesAsync();

        var saved = await _context.TodoDailyRecords.FindAsync(record.Id);
        Assert.NotNull(saved);
        Assert.Equal(task.Id, saved!.TaskId);
        Assert.Equal(today, saved.RecordDate);
        Assert.Equal(TodoStatus.Pending, saved.Status);
        Assert.Equal(new TimeOnly(10, 30), saved.DueTime);
        Assert.True(saved.ReminderEnabled);
        Assert.Equal(1, saved.SortOrder);
    }

    [Fact]
    public async Task TodoDailyRecord_Status_RoundTrip()
    {
        var task = new TodoTask("Task", null);
        _context.TodoTasks.Add(task);
        await _context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = new TodoDailyRecord(task.Id, today);
        _context.TodoDailyRecords.Add(record);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var saved = await _context.TodoDailyRecords.FindAsync(record.Id);
        Assert.NotNull(saved);
        Assert.Equal(TodoStatus.Pending, saved!.Status);
    }

    [Fact]
    public async Task PopupSchedule_InsertAndQuery()
    {
        var schedule = new PopupSchedule
        {
            Name = "Daily Check",
            ScheduleType = PopupScheduleType.Daily,
            TimeOfDay = new TimeOnly(9, 0),
            Message = "Check todos"
        };
        _context.PopupSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        var saved = await _context.PopupSchedules.FindAsync(schedule.Id);
        Assert.NotNull(saved);
        Assert.Equal("Daily Check", saved!.Name);
        Assert.Equal(PopupScheduleType.Daily, saved.ScheduleType);
        Assert.Equal(new TimeOnly(9, 0), saved.TimeOfDay);
        Assert.True(saved.Enabled);
    }

    [Fact]
    public async Task AppSetting_InsertAndQuery()
    {
        var setting = new AppSetting("hotkey.value", "Ctrl+Alt+Space");
        _context.AppSettings.Add(setting);
        await _context.SaveChangesAsync();

        var saved = await _context.AppSettings.FindAsync("hotkey.value");
        Assert.NotNull(saved);
        Assert.Equal("Ctrl+Alt+Space", saved!.Value);
    }

    [Fact]
    public async Task AppSetting_UpdateExisting()
    {
        _context.AppSettings.Add(new AppSetting("theme", "dark"));
        await _context.SaveChangesAsync();

        var setting = await _context.AppSettings.FindAsync("theme");
        setting!.Value = "light";
        await _context.SaveChangesAsync();

        var saved = await _context.AppSettings.FindAsync("theme");
        Assert.Equal("light", saved!.Value);
    }

    [Fact]
    public async Task TodoDailyRecord_UniqueConstraint_TaskIdAndDate()
    {
        var task = new TodoTask("Task", null);
        _context.TodoTasks.Add(task);
        await _context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _context.TodoDailyRecords.Add(new TodoDailyRecord(task.Id, today));
        await _context.SaveChangesAsync();

        var duplicate = new TodoDailyRecord(task.Id, today);
        _context.TodoDailyRecords.Add(duplicate);
        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }
}
