using System.Text.Json;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

public class EquipmentDatabase
{
    public List<EquipmentItem> Items { get; private set; } = new();
    public string CurrentDatabase { get; private set; } = "";  // ← добавить эту строку
    /// <summary>
    /// Загрузить конкретную базу данных.
    /// </summary>
    public async Task LoadAsync(string webRootPath, string databaseFileName)
    {
        if (CurrentDatabase == databaseFileName && Items.Count > 0)
            return;

        string path = Path.Combine(webRootPath, "Configs", databaseFileName);
        if (!File.Exists(path))
        {
            Items = new();
            CurrentDatabase = databaseFileName;
            return;
        }

        string json = await File.ReadAllTextAsync(path);

        // Если файл пустой — создаём пустой список
        if (string.IsNullOrWhiteSpace(json))
        {
            Items = new();
            CurrentDatabase = databaseFileName;
            return;
        }

        try
        {
            // Пробуем как EquipmentItem (terminals-base.json и подобные)
            Items = JsonSerializer.Deserialize<List<EquipmentItem>>(json) ?? new();
        }
        catch (JsonException)
        {
            // Пробуем как старый формат JsonTerminalItem
            try
            {
                var legacyItems = JsonSerializer.Deserialize<List<UniversalCalculationEngine.JsonTerminalItem>>(json);
                Items = legacyItems?.Select(i => new EquipmentItem
                {
                    Article = i.Article,
                    Name = i.Name,
                    Unit = i.Unit,
                    Vendor = i.Vendor,
                    TerminalType = i.TerminalType,
                    WireSection = i.WireSection
                }).ToList() ?? new();
            }
            catch (JsonException)
            {
                // Не удалось распознать ни один формат — оставляем пустой список
                // Это не ошибка, просто база не того формата (например, power-cabinet.json)
                Items = new();
            }
        }

        CurrentDatabase = databaseFileName;
    }

    /// <summary>
    /// Найти оборудование по группе и ключевым словам в наименовании.
    /// </summary>
    public EquipmentItem? Find(string group, params string[] keywords)
    {
        return Items
            .Where(i => string.Equals(i.TerminalType, group, StringComparison.OrdinalIgnoreCase))
            .Where(i => keywords.All(k => i.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault();
    }

    /// <summary>
    /// Найти все позиции по группе.
    /// </summary>
    public List<EquipmentItem> FindAll(string group)
    {
        return Items
            .Where(i => string.Equals(i.TerminalType, group, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}