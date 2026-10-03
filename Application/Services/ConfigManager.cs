using System.Text.Json;
using Application.Models;

namespace Application.Services;

public class ConfigManager
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "KnotNotes",
        "settings.json"
    );

    public AppConfig LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            return new AppConfig();
        }

        string json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

    public void SaveConfig(AppConfig config)
    {
        string dir = Path.GetDirectoryName(ConfigPath);
        Directory.CreateDirectory(dir);
        
        string json = JsonSerializer.Serialize(config, new JsonSerializerOptions(){WriteIndented = true});
        File.WriteAllText(ConfigPath, json);
    }
}