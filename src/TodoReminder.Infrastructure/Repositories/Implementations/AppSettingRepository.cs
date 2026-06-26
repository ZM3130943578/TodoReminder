using Microsoft.EntityFrameworkCore;
using TodoReminder.Domain.Entities;
using TodoReminder.Infrastructure.Database;
using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.Infrastructure.Repositories.Implementations;

public class AppSettingRepository : IAppSettingRepository
{
    private readonly AppDbContext _context;

    public AppSettingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _context.AppSettings.FindAsync(key);
        return setting?.Value;
    }

    public async Task SetValueAsync(string key, string value)
    {
        var existing = await _context.AppSettings.FindAsync(key);
        if (existing != null)
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.AppSettings.Add(new AppSetting(key, value));
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<KeyValuePair<string, string>>> GetAllAsync()
    {
        return await _context.AppSettings
            .Select(s => new KeyValuePair<string, string>(s.Key, s.Value))
            .ToListAsync();
    }
}
