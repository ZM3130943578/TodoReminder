using TodoReminder.Domain.Entities;

namespace TodoReminder.Infrastructure.Repositories.Interfaces;

public interface IPopupScheduleRepository
{
    Task<PopupSchedule?> GetByIdAsync(Guid id);
    Task<List<PopupSchedule>> GetAllAsync();
    Task<List<PopupSchedule>> GetEnabledAsync();
    Task<PopupSchedule> AddAsync(PopupSchedule schedule);
    Task UpdateAsync(PopupSchedule schedule);
    Task DeleteAsync(Guid id);
}
