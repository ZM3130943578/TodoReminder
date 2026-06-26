using Microsoft.EntityFrameworkCore;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;
using TodoReminder.Infrastructure.Database;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.Infrastructure.Repositories.Implementations;

public class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;

    public TodoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TodoTask?> GetTaskByIdAsync(Guid id)
    {
        return await _context.TodoTasks
            .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
    }

    public async Task<TodoTask> AddTaskAsync(TodoTask task)
    {
        _context.TodoTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateTaskAsync(TodoTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.TodoTasks.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task<TodoDailyRecord?> GetRecordByIdAsync(Guid id)
    {
        return await _context.TodoDailyRecords
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<TodoDailyRecord>> GetRecordsByDateAsync(DateOnly date)
    {
        return await _context.TodoDailyRecords
            .Where(r => r.RecordDate == date)
            .OrderBy(r => r.SortOrder)
            .ToListAsync();
    }

    public async Task<List<TodoDailyRecord>> GetRecordsByDateRangeAsync(DateOnly from, DateOnly to)
    {
        return await _context.TodoDailyRecords
            .Where(r => r.RecordDate >= from && r.RecordDate <= to)
            .OrderBy(r => r.RecordDate)
            .ThenBy(r => r.SortOrder)
            .ToListAsync();
    }

    public async Task<TodoDailyRecord> AddRecordAsync(TodoDailyRecord record)
    {
        _context.TodoDailyRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task UpdateRecordAsync(TodoDailyRecord record)
    {
        record.UpdatedAt = DateTime.UtcNow;
        _context.TodoDailyRecords.Update(record);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRecordAsync(Guid id)
    {
        var record = await _context.TodoDailyRecords.FindAsync(id);
        if (record != null)
        {
            _context.TodoDailyRecords.Remove(record);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<TodoDailyRecord>> GetPendingRecordsBeforeDateAsync(DateOnly date)
    {
        return await _context.TodoDailyRecords
            .Where(r => r.RecordDate < date && r.Status == TodoStatus.Pending)
            .OrderBy(r => r.RecordDate)
            .ToListAsync();
    }

    public async Task<DateOnly?> GetMostRecentRecordDateAsync(DateOnly before)
    {
        var dates = await _context.TodoDailyRecords
            .Where(r => r.RecordDate < before)
            .Select(r => r.RecordDate)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();
        return dates.FirstOrDefault();
    }
}
