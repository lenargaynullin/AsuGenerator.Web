using System;
using System.Collections.Generic;
using System.IO;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services.Strategies.Implementations;

/// <summary>
/// Стратегия подбора шкафа управления вентиляцией (ШУВ).
/// Использует основной конструктор (Primary Constructor) C# 12+.
/// </summary>
public class ShuvStrategy(ShuvConfigLoader loader, SupplierDatabase supplierDb) : ICabinetStrategy
{
    public string CabinetType => "Шкаф управления вентиляцией (ШУВ)";

    public List<SelectedComponent> CalculateComponents(UiConfigInput input)
    {
        try
        {
            if (input == null)
                return [new() { Designation = "ERR", Article = "ERR", Description = "input == null", Vendor = "ERR" }];

            // Кроссплатформенный сборщик путей
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "Configs", "shuv-strategy.json");

            // Защита от регистра имен папок в Linux/Docker окружении
            if (!File.Exists(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "configs", "shuv-strategy.json");
            }

            var config = loader.Load(path);

            if (config?.Devices == null || config.Devices.Count == 0)
                return [new() { Designation = "ERR", Article = "ERR", Description = "Devices пуст", Vendor = "ERR" }];

            var components = new List<SelectedComponent>();
            string brand = input.BaseConfig?.PreferredBrand ?? "KEAZ";
            var counters = new Dictionary<string, int>();

            foreach (var device in config.Devices)
            {
                if (device.Condition != null)
                {
                    if (!CheckCondition(device.Condition, input))
                        continue;
                }

                string article = ResolveArticle(device, brand);
                string designation = AutoNumber(device.Designation, counters);

                components.Add(new SelectedComponent
                {
                    Designation = designation,
                    Article = article,
                    Vendor = brand, // Перезаписываем бренд на выбранный пользователем на экране
                    Description = device.Description,
                    Quantity = device.Quantity
                });
            }

            return components;
        }
        catch (Exception ex)
        {
            return [new() { Designation = "ERR", Article = "ERR", Description = ex.Message, Vendor = ex.GetType().Name }];
        }
    }

    /// <summary>
    /// Проверка условий включения модулей. Сделана статической для повышения производительности.
    /// </summary>
    private static bool CheckCondition(DeviceCondition condition, UiConfigInput input)
    {
        if (condition == null || string.IsNullOrEmpty(condition.Field)) return true;

        return condition.Field switch
        {
            "heaterType" => input.TechnologyType == condition.Value,
            "hasHumidifier" => input.HasHumidifier == (condition.Value == "true"),
            "hasReserveFan" => input.HasReserveFan == (condition.Value == "true"),
            "hasDispatching" => input.HasDispatching == (condition.Value == "true"),
            "hasAdditionalSensors" => input.HasAdditionalSensors == (condition.Value == "true"),
            "hasHeater" => (input.BaseConfig?.HasHeater == true) == (condition.Value == "true"),
            "hasModbus" => (input.BaseConfig?.Protocol?.Contains("Modbus") == true) == (condition.Value == "true"),
            _ => false
        };
    }

    /// <summary>
    /// Автоматическое нумерование позиций по ГОСТ (KM1, KM2...). Помечено как static.
    /// </summary>
    private static string AutoNumber(string baseDesignation, Dictionary<string, int> counters)
    {
        if (!counters.TryGetValue(baseDesignation, out int value))
            value = 0;

        value++;
        counters[baseDesignation] = value;
        return $"{baseDesignation}{value}";
    }

    private string ResolveArticle(DeviceConfig device, string brand)
    {
        if (device.Params == null)
            return supplierDb.FindArticle(brand, device.DeviceType, null);

        var requiredParams = new Dictionary<string, string>();

        switch (device.DeviceType)
        {
            case "Breaker":
                if (device.Params.Poles > 0) requiredParams["poles"] = device.Params.Poles.ToString();
                if (device.Params.RatedCurrent > 0) requiredParams["current"] = device.Params.RatedCurrent.ToString();
                break;

            case "Lamp" or "Button":
                if (!string.IsNullOrEmpty(device.Params.Color)) requiredParams["color"] = device.Params.Color;
                break;

            case "Relay" when device.Params.Voltage > 0:
                requiredParams["voltage"] = device.Params.Voltage.ToString();
                break;

            case "Terminal" when device.Params.Section > 0:
                requiredParams["section"] = device.Params.Section.ToString();
                break;

            case "Contactor" when device.Params.Poles > 0:
                requiredParams["poles"] = device.Params.Poles.ToString();
                break;
        }

        return supplierDb.FindArticle(brand, device.DeviceType, requiredParams.Count > 0 ? requiredParams : null);
    }
}
