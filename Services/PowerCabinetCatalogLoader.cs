using System.IO;
using System.Text.Json;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

/// <summary>
/// Загрузчик каталога IEK для конфигуратора силовых шкафов
/// (аналог ShuvConfigLoader / singleton PlcBaseRoot).
/// </summary>
public class PowerCabinetCatalogLoader
{
    public PowerCabinetCatalog Load(IWebHostEnvironment env)
    {
        string jsonPath = Path.Combine(env.WebRootPath, "Configs", "power-cabinet.json");
        if (!File.Exists(jsonPath))
            return new PowerCabinetCatalog();

        var json = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<PowerCabinetCatalog>(json, options) ?? new PowerCabinetCatalog();
    }
}
