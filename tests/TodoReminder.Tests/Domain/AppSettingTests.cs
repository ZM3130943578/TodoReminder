using TodoReminder.Domain.Entities;

namespace TodoReminder.Tests.Domain;

public class AppSettingTests
{
    [Fact]
    public void Create_ShouldSetKeyAndValue()
    {
        var setting = new AppSetting("hotkey.value", "Ctrl+Alt+Space");

        Assert.Equal("hotkey.value", setting.Key);
        Assert.Equal("Ctrl+Alt+Space", setting.Value);
        Assert.NotEqual(default, setting.UpdatedAt);
    }

    [Fact]
    public void Update_ShouldChangeValue()
    {
        var setting = new AppSetting("window.topmost", "true");
        setting.Value = "false";
        setting.UpdatedAt = DateTime.UtcNow;

        Assert.Equal("false", setting.Value);
    }

    [Fact]
    public void DefaultConstructor_ShouldUseEmptyStrings()
    {
        var setting = new AppSetting();

        Assert.Equal(string.Empty, setting.Key);
        Assert.Equal(string.Empty, setting.Value);
    }
}
