using Microsoft.EntityFrameworkCore;
using TodoReminder.Domain.Entities;
using TodoReminder.Domain.Enums;

namespace TodoReminder.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public DbSet<TodoTask> TodoTasks => Set<TodoTask>();
    public DbSet<TodoDailyRecord> TodoDailyRecords => Set<TodoDailyRecord>();
    public DbSet<PopupSchedule> PopupSchedules => Set<PopupSchedule>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(2000);
        });

        modelBuilder.Entity<TodoDailyRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();
            entity.HasIndex(e => new { e.TaskId, e.RecordDate }).IsUnique();
        });

        modelBuilder.Entity<PopupSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ScheduleType)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(200);
        });
    }
}
