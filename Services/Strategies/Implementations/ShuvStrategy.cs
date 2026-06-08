using System;
using System.Collections.Generic;
using System.IO;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services.Strategies.Implementations;

/// <summary>
/// Стратегия подбора шкафа управления вентиляцией (ШУВ).
/// Использует основной конструктор (Primary Constructor) C# 12+.
/// </summary>
public class ShuvStrategy : ICabinetStrategy
{
    private readonly ShuvConfigLoader _loader;

    public ShuvStrategy(ShuvConfigLoader loader)
    {
        _loader = loader;
    }

    public string CabinetType => "Шкаф управления вентиляцией (ШУВ)";

    public List<SelectedComponent> CalculateComponents(UiConfigInput input)
    {
        try
        {
            if (input == null)
                return new List<SelectedComponent> { new() { Designation = "ERR", Article = "ERR", Description = "input == null", Vendor = "ERR" } };

            var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "Configs", "shuv-strategy.json");
            var config = _loader.Load(path);

            if (config?.Rules == null || config.Rules.Count == 0)
                return new List<SelectedComponent> { new() { Designation = "ERR", Article = "ERR", Description = "Devices пуст", Vendor = "ERR" } };

            var components = new List<SelectedComponent>();

            // Глобальные счетчики для чистых буквенных кодов ГОСТ (QF, KM, HL)
            var counters = new Dictionary<string, int>();

            // Словари для связи индексов аксессуаров с основным устройством по условию активации
            var contextIndexes = new Dictionary<string, int>();

            foreach (var device in config.Rules)
            {
                if (!CheckCondition(device.Condition, input))
                    continue;

                // 1. Извлекаем чистый буквенный код ГОСТ (все что до символа '_')
                string rawDesignation = device.Designation;
                string baseCode = rawDesignation.Contains('_') ? rawDesignation.Split('_')[0] : rawDesignation;

                int currentIndex;
                string contextKey = $"{baseCode}_{device.Condition}";

                // ИСПРАВЛЕНИЕ 1 & 2: Разделяем аксессуары и самостоятельные устройства типа ламп/кнопок
                bool isAccessory = rawDesignation.Contains('_');
                bool isAlwaysCondition = device.Condition == "Always" || device.Condition == "Всегда";

                if (baseCode == "XT")
                {
                    // Клеммные группы стартуют с 1 (XT1), заглушки без номеров
                    currentIndex = 1;
                }
                // Если это аксессуар (колодка/зажим) ИЛИ устройство привязано к логике (вентилятор/заслонка) - шерим индекс
                else if (contextIndexes.ContainsKey(contextKey) && (isAccessory || !isAlwaysCondition))
                {
                    currentIndex = contextIndexes[contextKey];
                }
                else
                {
                    // Для независимых приборов (даже с одинаковым "Always") строго растим счетчик вверх
                    if (!counters.ContainsKey(baseCode)) counters[baseCode] = 0;
                    counters[baseCode]++;
                    currentIndex = counters[baseCode];

                    // Запоминаем этот индекс для последующих аксессуаров в этом же блоке
                    contextIndexes[contextKey] = currentIndex;
                }

                // 2. Форматируем финальное позиционное обозначение
                string finalDesignation = FormatDesignation(rawDesignation, baseCode, currentIndex);

                // 3. ИСПРАВЛЕНИЕ 3: Надежная подмена бренда (проверяем вхождение слова "Авт" или "Выключатель")
                string vendor = device.DefaultSupplier;
                string article = device.Article;

                string categoryLower = device.Category?.ToLower() ?? "";
                bool isModularProtection = categoryLower.Contains("авт") || categoryLower.Contains("выключатель");

                if (isModularProtection && !string.IsNullOrWhiteSpace(input.PreferredBrand))
                {
                    vendor = input.PreferredBrand;
                    if (vendor == "КЭАЗ")
                    {
                        // Берем уставку тока, убирая лишние пробелы
                        string specParam = !string.IsNullOrEmpty(device.TechParameters) ? device.TechParameters.Split(' ')[0] : "ВА";
                        article = $"КЭАЗ-{specParam}";
                    }
                }

                components.Add(new SelectedComponent
                {
                    Designation = finalDesignation,
                    Article = article,
                    Vendor = vendor,
                    Description = device.Description,
                    Quantity = 1
                });
            }

            var groupedComponents = components
                .GroupBy(c => new { c.Designation, c.Article, c.Vendor, c.Description })
                .Select(g => new SelectedComponent
                {
                    Designation = g.Key.Designation,
                    Article = g.Key.Article,
                    Vendor = g.Key.Vendor,
                    Description = g.Key.Description,
                    Quantity = g.Count() // Считаем количество дубликатов
                })
                .ToList();

            return groupedComponents;
        }
        catch (Exception ex)
        {
            return new List<SelectedComponent> { new() { Designation = "ERR", Article = "ERR", Description = ex.Message, Vendor = ex.GetType().Name } };
        }
    }

    private static bool CheckCondition(string condition, UiConfigInput input)
    {
        if (condition == "Always" || condition == "Всегда") return true;

        return condition switch
        {
            "Приточный вентилятор" => input.SupplyFan,
            "SupplyFan" => input.SupplyFan,
            "SupplyFan && HasVfd" => input.SupplyFan && input.HasVfd,
            "SupplyFan && !HasVfd" => input.SupplyFan && !input.HasVfd,
            "SupplyFan && HasVfd && !HasHeaterPump" => input.SupplyFan && input.HasVfd && !input.HasHeaterPump,
            "Насос калорифера" => input.HasHeaterPump,
            "HasHeaterPump" => input.HasHeaterPump,
            "Насос увлажнителя" => input.HasHumidifierPump,
            "Заслонка приточная" => input.HasDamper,
            "HasDamper" => input.HasDamper,
            "3-х ходовой клапан" => input.HasHeaterPump, // Клапан идет вместе с калорифером
            _ => false
        };
    }

    private static string FormatDesignation(string raw, string baseCode, int index)
    {
        if (!raw.Contains('_'))
            return $"{baseCode}{index}"; // Стандартное устройство: QF1, KM2, HL3

        // Обработка аксессуаров на основе суффиксов
        return raw switch
        {
            _ when raw.EndsWith("_Socket") => $"Колодка для {baseCode}{index}",
            _ when raw.EndsWith("_Clip") => $"Зажим для {baseCode}{index}",
            _ when raw.EndsWith("_Led") => $"Индикатор {baseCode}{index}",
            _ when raw.EndsWith("_Block") => $"Контакт для {baseCode}{index}",
            _ when raw.EndsWith("_End") => $"Заглушка {baseCode}",
            _ when raw.EndsWith("_Common") => $"{baseCode} (Общие)",
            _ => $"{baseCode}{index}"
        };
    }
}
