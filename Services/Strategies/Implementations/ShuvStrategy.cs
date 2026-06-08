using System;
using System.Collections.Generic;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services.Strategies.Implementations;

public class ShuvStrategy : ICabinetStrategy
{
    private readonly ShuvConfigLoader _loader;
    private readonly SupplierDatabase _supplierDb;

    public ShuvStrategy(ShuvConfigLoader loader, SupplierDatabase supplierDb)
    {
        _loader = loader;
        _supplierDb = supplierDb;
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

            if (config?.Devices == null || config.Devices.Count == 0)
                return new List<SelectedComponent> { new() { Designation = "ERR", Article = "ERR", Description = "Devices пуст", Vendor = "ERR" } };

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
                    Vendor = device.Vendor,
                    Description = device.Description,
                    Quantity = device.Quantity
                });
            }

            return components;
        }
        catch (Exception ex)
        {
            return new List<SelectedComponent> { new() { Designation = "ERR", Article = "ERR", Description = ex.Message, Vendor = ex.GetType().Name } };
        }
    }

    private bool CheckCondition(DeviceCondition condition, UiConfigInput input)
    {
        if (condition.Field == "heaterType")
            return input.TechnologyType == condition.Value;
        return false;
    }

    private string AutoNumber(string baseDesignation, Dictionary<string, int> counters)
    {
        if (!counters.ContainsKey(baseDesignation))
            counters[baseDesignation] = 0;
        counters[baseDesignation]++;
        return $"{baseDesignation}{counters[baseDesignation]}";
    }

    private string ResolveArticle(DeviceConfig device, string brand)
    {
        if (device.Params == null)
            return _supplierDb.FindArticle(brand, device.DeviceType, null);

        var requiredParams = new Dictionary<string, string>();

        if (device.DeviceType == "Breaker")
        {
            if (device.Params.Poles > 0) requiredParams["poles"] = device.Params.Poles.ToString();
            if (device.Params.RatedCurrent > 0) requiredParams["current"] = device.Params.RatedCurrent.ToString();
        }
        else if (device.DeviceType == "Lamp" && !string.IsNullOrEmpty(device.Params.Color))
        {
            requiredParams["color"] = device.Params.Color;
        }
        else if (device.DeviceType == "Button" && !string.IsNullOrEmpty(device.Params.Color))
        {
            requiredParams["color"] = device.Params.Color;
        }
        else if (device.DeviceType == "Relay" && device.Params.Voltage > 0)
        {
            requiredParams["voltage"] = device.Params.Voltage.ToString();
        }
        else if (device.DeviceType == "Terminal" && device.Params.Section > 0)
        {
            requiredParams["section"] = device.Params.Section.ToString();
        }
        else if (device.DeviceType == "Contactor" && device.Params.Poles > 0)
        {
            requiredParams["poles"] = device.Params.Poles.ToString();
        }

        return _supplierDb.FindArticle(brand, device.DeviceType, requiredParams.Count > 0 ? requiredParams : null);
    }
}