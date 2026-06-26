using Microsoft.EntityFrameworkCore;
using TodoReminder.Domain.Entities;
using TodoReminder.Infrastructure.Database;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.Infrastructure.Repositories.Implementations;

public class PopupScheduleRepository : IPopupScheduleRepository
{
    private readonly AppDbContext _context;

    public PopupScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PopupSchedule?> GetByIdAsync(Guid id)
    {
        return await _context.PopupSchedules.FindAsync(id);
    }

    public async Task<List<PopupSchedule>> GetAllAsync()
    {
        return await _context.PopupSchedules
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PopupSchedule>> GetEnabledAsync()
    {
        return await _context.PopupSchedules
            .Where(s => s.Enabled)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<PopupSchedule> AddAsync(PopupSchedule schedule)
    {
        _context.PopupSchedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task UpdateAsync(PopupSchedule schedule)
    {
        schedule.UpdatedAt = DateTime.UtcNow;
        _context.PopupSchedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var schedule = await _context.PopupSchedules.FindAsync(id);
        if (schedule != null)
        {
            _context.PopupSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
