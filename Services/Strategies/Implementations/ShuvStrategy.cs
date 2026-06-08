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
            var counters = new Dictionary<string, int>();

            foreach (var device in config.Rules)
            {
                if (!CheckCondition(device.Condition, input))
                    continue;

                string designation = AutoNumber(device.Designation, counters);

                components.Add(new SelectedComponent
                {
                    Designation = designation,
                    Article = device.Article,
                    Vendor = device.DefaultSupplier,
                    Description = device.Description,
                    Quantity = 1
                });
            }

            return components;
        }
        catch (Exception ex)
        {
            return new List<SelectedComponent> { new() { Designation = "ERR", Article = "ERR", Description = ex.Message, Vendor = ex.GetType().Name } };
        }
    }

    private static bool CheckCondition(string condition, UiConfigInput input)
    {
        if (condition == "Always") return true;

        return condition switch
        {
            "SupplyFan" => input.SupplyFan,
            "SupplyFan && HasVfd" => input.SupplyFan && input.HasVfd,
            "SupplyFan && !HasVfd" => input.SupplyFan && !input.HasVfd,
            "SupplyFan && HasVfd && !HasHeaterPump" => input.SupplyFan && input.HasVfd && !input.HasHeaterPump,
            "HasDamper" => input.HasDamper,
            "HasHeaterPump" => input.HasHeaterPump,
            _ => false
        };
    }

    private static string AutoNumber(string baseDesignation, Dictionary<string, int> counters)
    {
        if (!counters.ContainsKey(baseDesignation))
            counters[baseDesignation] = 0;
        counters[baseDesignation]++;
        return $"{baseDesignation}{counters[baseDesignation]}";
    }
}
