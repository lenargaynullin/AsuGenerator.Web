using System.IO;
using System.Text.Json;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

public class ShuvConfigLoader
{
    public ShuvConfig Load(string configPath)
    {
        var json = File.ReadAllText(configPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true  // ← ДОБАВИТЬ
        };
        var config = JsonSerializer.Deserialize<ShuvConfig>(json, options);
        return config ?? new ShuvConfig();
    }
}