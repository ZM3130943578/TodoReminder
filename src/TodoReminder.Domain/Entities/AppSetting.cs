namespace TodoReminder.Domain.Entities;

public class AppSetting
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }

    public AppSetting()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public AppSetting(string key, string value) : this()
    {
        Key = key;
        Value = value;
    }
}
