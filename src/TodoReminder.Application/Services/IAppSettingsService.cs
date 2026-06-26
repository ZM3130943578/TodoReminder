namespace TodoReminder.Application.Services;

public interface IAppSettingsService
{
    string? GetValue(string key);
    void SetValue(string key, string value);
    T? GetValue<T>(string key) where T : struct;
    void SetValue<T>(string key, T value) where T : struct;
}
