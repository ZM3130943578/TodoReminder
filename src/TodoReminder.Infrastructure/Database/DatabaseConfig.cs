using Microsoft.EntityFrameworkCore;

namespace TodoReminder.Infrastructure.Database;

public static class DatabaseConfig
{
    public static string GetDefaultConnectionString()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TodoReminderTool");
        Directory.CreateDirectory(folder);
        return $"Data Source={Path.Combine(folder, "todo.db")}";
    }

    public static DbContextOptions<AppDbContext> CreateOptions(string? connectionString = null)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString ?? GetDefaultConnectionString())
            .Options;
    }
}
