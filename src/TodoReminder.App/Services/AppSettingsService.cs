using TodoReminder.Infrastructure.Repositories.Interfaces;

namespace TodoReminder.App.Services;

public class AppSettingsService
{
    private readonly IAppSettingRepository _repository;
    private Dictionary<string, string> _settings = new();

    public AppSettingsService(IAppSettingRepository repository)
    {
        _repository = repository;
    }

    public async Task LoadAsync()
    {
        var all = await _repository.GetAllAsync();
        _settings = new Dictionary<string, string>(all, StringComparer.OrdinalIgnoreCase);
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (_settings.TryGetValue(key, out var value) && bool.TryParse(value, out var result))
            return result;
        return defaultValue;
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        if (_settings.TryGetValue(key, out var value) && int.TryParse(value, out var result))
            return result;
        return defaultValue;
    }

    public string GetString(string key, string defaultValue = "")
    {
        return _settings.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public async Task SaveAllAsync(Dictionary<string, string> settings)
    {
        foreach (var kvp in settings)
        {
            await _repository.SetValueAsync(kvp.Key, kvp.Value);
            _settings[kvp.Key] = kvp.Value;
        }
    }
}
