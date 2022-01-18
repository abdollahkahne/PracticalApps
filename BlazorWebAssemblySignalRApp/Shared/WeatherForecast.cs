namespace BlazorWebAssemblySignalRApp.Shared;

public class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
public interface IUserService
{
    string? Name { get; }
}

public class Setting
{
    public string? SettingName { get; set; }
    public string? SettingValue { get; set; }
}

public interface ISettingService
{
    IList<Setting> GetSettings();
}

public class UserService : IUserService
{
    private static readonly string _value = new Random().NextInt64().ToString();
    public string Name => "User Name is Random:" + _value;
}

public class SettingService : ISettingService
{
    private static string _color = new Random().NextInt64().ToString();
    private readonly List<Setting> _settings = new()
    {
        new Setting { SettingName = "BackgroundColor", SettingValue = "Blue" },
        new Setting { SettingName = "RandomColor", SettingValue = "Blue" + _color },
        new Setting { SettingName = "TextColor", SettingValue = "Yellow" },
        new Setting { SettingName = "ForeColor", SettingValue = "Red" },
    };

    public IList<Setting> GetSettings()
    {
        return _settings;
    }
}