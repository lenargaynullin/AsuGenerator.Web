using System.Collections.Generic;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

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
            // 1. Защита от null для входного параметра
            if (input == null)
            {
                return new List<SelectedComponent>
            {
                new() { Designation = "ERR", Article = "ERR", Description = "Входные данные input равны null", Vendor = "ERR" }
            };
            }

            var path = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "wwwroot", "Configs", "shuv-strategy.json");

            System.Diagnostics.Debug.WriteLine($"[DIAG] Путь к JSON: {path}");
            System.Diagnostics.Debug.WriteLine($"[DIAG] Файл существует: {System.IO.File.Exists(path)}");

            var config = _loader.Load(path);

            // 2. Единая строгая проверка конфигурации
            if (config?.CommonDevices == null || config.CommonDevices.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[DIAG] Config или CommonDevices пуст!");
                return new List<SelectedComponent>
            {
                new() { Designation = "ERR", Article = "ERR", Description = "CommonDevices пуст", Vendor = "ERR" }
            };
            }

            System.Diagnostics.Debug.WriteLine($"[DIAG] Config загружен: true");
            System.Diagnostics.Debug.WriteLine($"[DIAG] CommonDevices count: {config.CommonDevices.Count}");
            System.Diagnostics.Debug.WriteLine($"[DIAG] Rules count: {config.Rules?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"[DIAG] Devices count: {config.Devices?.Count ?? 0}");

            // Элементы гарантированно не null благодаря проверке выше
            var selectedDesignations = new HashSet<string>(config.CommonDevices);
            System.Diagnostics.Debug.WriteLine($"[DIAG] selectedDesignations после CommonDevices: {selectedDesignations.Count}");

            // Применяем правила
            ApplyRule(selectedDesignations, config, "heaterType", input.TechnologyType ?? "");

            System.Diagnostics.Debug.WriteLine($"[DIAG] selectedDesignations после правил: {selectedDesignations.Count}");
            foreach (var d in selectedDesignations)
                System.Diagnostics.Debug.WriteLine($"[DIAG]   {d}");

            var components = new List<SelectedComponent>();

            // 3. Безопасный дефолт для бренда
            string brand = input.BaseConfig?.PreferredBrand ?? "KEAZ";

            if (config.Devices != null)
            {
                foreach (var key in selectedDesignations)
                {
                    // Исключаем попадание null-ключей
                    if (key != null && config.Devices.TryGetValue(key, out var device) && device != null)
                    {
                        string article = ResolveArticle(device, brand);
                        components.Add(new SelectedComponent
                        {
                            Designation = device.Designation,
                            Article = article,
                            Vendor = device.Vendor,
                            Description = device.Description,
                            Quantity = device.Quantity
                        });
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"[DIAG] Итого компонентов: {components.Count}");
            return components;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DIAG] ОШИБКА: {ex.Message}");
            return new List<SelectedComponent>
        {
            new() { Designation = "ERR", Article = "ERR", Description = ex.Message, Vendor = ex.GetType().Name }
        };
        }
    }

    /*public List<SelectedComponent> CalculateComponents(UiConfigInput input)
    {
        var config = _loader.Load("wwwroot/Configs/shuv-strategy.json");
        var selectedDesignations = new HashSet<string>(config.CommonDevices);

        // Применяем правила
        ApplyRule(selectedDesignations, config, "heaterType", input.TechnologyType ?? "");
        ApplyRule(selectedDesignations, config, "hasHumidifier", BoolToKey(input.HasHumidifier));
        ApplyRule(selectedDesignations, config, "hasReserveFan", BoolToKey(input.HasReserveFan));
        ApplyRule(selectedDesignations, config, "hasDispatching", BoolToKey(input.HasDispatching));
        ApplyRule(selectedDesignations, config, "hasAdditionalSensors", BoolToKey(input.HasAdditionalSensors));
        ApplyRule(selectedDesignations, config, "hasHeater", BoolToKey(input.BaseConfig?.HasHeater == true));
        ApplyRule(selectedDesignations, config, "hasModbus", BoolToKey(input.BaseConfig?.Protocol?.Contains("Modbus") == true));

        // Превращаем ключи в компоненты с артикулами
        var components = new List<SelectedComponent>();
        foreach (var key in selectedDesignations)
        {
            if (config.Devices.TryGetValue(key, out var device))
            {
                string article = ResolveArticle(device, input.BaseConfig?.PreferredBrand ?? "KEAZ");

                components.Add(new SelectedComponent
                {
                    Designation = device.Designation,
                    Article = article,
                    Vendor = device.Vendor,
                    Description = device.Description,
                    Quantity = device.Quantity
                });
            }
        }

        return components;
    }
    */

    private void ApplyRule(HashSet<string> target, ShuvConfig config, string ruleKey, string inputValue)
    {
        if (string.IsNullOrEmpty(inputValue)) return;

        if (config.Rules.TryGetValue(ruleKey, out var rule) &&
            rule.TryGetValue(inputValue, out var devices))
        {
            foreach (var d in devices)
                target.Add(d);
        }
    }

    private string BoolToKey(bool value) => value ? "true" : "";

    private string ResolveArticle(DeviceConfig device, string brand)
    {
        if (device.DeviceType == "Breaker" && device.Params != null)
        {
            return _supplierDb.FindBreakerArticle(brand, new BreakerParams
            {
                Poles = device.Params.Poles,
                RatedCurrent = device.Params.RatedCurrent
            });
        }

        // Все остальные типы устройств
        return _supplierDb.FindDeviceArticle(brand, device.DeviceType, device.Params);
    }

    // Остальные методы интерфейса — заглушки
    public CommercialProposal CalculateProposal(List<SelectedComponent> components, UiConfigInput input, decimal margin, PriceCalculationService priceCalc)
        => new() { ProjectName = "ШУВ", ClientName = input.BaseConfig?.ClientName ?? "" };

    public Dictionary<string, byte[]> GenerateCadDrawings(List<SelectedComponent> components, UiConfigInput input, CadGeneratorService cadGen)
        => new();
}