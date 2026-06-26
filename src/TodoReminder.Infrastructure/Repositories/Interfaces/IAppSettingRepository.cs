namespace TodoReminder.Infrastructure.Repositories.Interfaces;

public interface IAppSettingRepository
{
    Task<string?> GetValueAsync(string key);
    Task SetValueAsync(string key, string value);
    Task<List<KeyValuePair<string, string>>> GetAllAsync();
}
