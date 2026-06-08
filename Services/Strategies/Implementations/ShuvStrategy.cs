using System;
using System.Collections.Generic;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services.Strategies.Implementations;

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
                return [new() { Designation = "ERR", Article = "ERR", Description = "input == null", Vendor = "ERR" }];

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "Configs", "shuv-strategy.json");
            var config = _loader.Load(path);

            if (config?.Rules == null || config.Rules.Count == 0)
                return [new() { Designation = "ERR", Article = "ERR", Description = "Rules пуст", Vendor = "ERR" }];

            var components = new List<SelectedComponent>();
            var counters = new Dictionary<string, int>();
            var contextIndexes = new Dictionary<string, int>();

            foreach (var device in config.Rules)
            {
                if (!CheckCondition(device.Condition, input))
                    continue;

                string raw = device.Designation;
                string baseCode = raw.Contains('_') ? raw.Split('_')[0] : raw;
                bool isAccessory = raw.Contains('_');
                string contextKey = $"{baseCode}_{device.Condition}";

                int index;

                if (isAccessory && contextIndexes.ContainsKey(contextKey))
                {
                    index = contextIndexes[contextKey];
                }
                else
                {
                    if (!counters.ContainsKey(baseCode))
                        counters[baseCode] = 0;
                    counters[baseCode]++;
                    index = counters[baseCode];

                    if (!isAccessory)
                        contextIndexes[contextKey] = index;
                }

                string designation = FormatDesignation(raw, baseCode, index);

                components.Add(new SelectedComponent
                {
                    Designation = designation,
                    Article = device.Article,
                    Vendor = device.DefaultSupplier,
                    Description = device.Description,
                    Quantity = device.Quantity
                });
            }

            // Группировка одинаковых артикулов
            var grouped = new List<SelectedComponent>();

            foreach (var comp in components)
            {
                var existing = grouped.FirstOrDefault(g => g.Article == comp.Article && g.Vendor == comp.Vendor);
                if (existing != null)
                {
                    existing.Quantity += comp.Quantity;
                    // Добавляем обозначение через запятую, если его ещё нет
                    if (!existing.Designation.Contains(comp.Designation))
                        existing.Designation += ", " + comp.Designation;
                }
                else
                {
                    grouped.Add(new SelectedComponent
                    {
                        Designation = comp.Designation,
                        Article = comp.Article,
                        Vendor = comp.Vendor,
                        Description = comp.Description,
                        Quantity = comp.Quantity
                    });
                }
            }

            return grouped;
        }
        catch (Exception ex)
        {
            return [new() { Designation = "ERR", Article = "ERR", Description = ex.Message, Vendor = ex.GetType().Name }];
        }
    }

    private static bool CheckCondition(string condition, UiConfigInput input)
    {
        if (condition == "Always" || condition == "Всегда") return true;

        return condition switch
        {
            "Приточный вентилятор" or "SupplyFan" => input.SupplyFan,
            "SupplyFan && HasVfd" => input.SupplyFan && input.HasVfd,
            "SupplyFan && !HasVfd" => input.SupplyFan && !input.HasVfd,
            "SupplyFan && HasVfd && !HasHeaterPump" => input.SupplyFan && input.HasVfd && !input.HasHeaterPump,
            "Насос калорифера" or "HasHeaterPump" => input.HasHeaterPump,
            "Насос увлажнителя" or "HasHumidifierPump" => input.HasHumidifierPump,
            "Заслонка приточная" or "HasDamper" => input.HasDamper,
            "3-х ходовой клапан" or "Has3WayValve" => input.Has3WayValve,
            "Авария увлажнитель" => input.HasHumidifierPump,
            "Авария блок кондиционера" => input.HasAirConditioner,
            "Пуск блок кондиционера" => input.HasAirConditioner,
            "Сигнал аварии" => input.SupplyFan,
            "DI на ЧП ПВ" => input.SupplyFan,
            "Т наружная" => input.SupplyFan,
            "Т воды" => input.HasHeaterPump,
            "ПВТ100" => input.SupplyFan,
            "Диспетчеризация" => input.HasDispatching,
            _ => false
        };
    }

    private static string FormatDesignation(string raw, string baseCode, int index)
    {
        if (!raw.Contains('_'))
            return $"{baseCode}{index}";

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