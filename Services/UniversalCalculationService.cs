using AsuGenerator.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Services;

/// <summary>
/// Универсальный сервис расчёта системы ПЛК для любого вендора из plc-base.json.
/// Заменяет разрозненные RegulCalculationService, PlcCalculationService и AbakCalculationService.
/// </summary>
public class UniversalCalculationService
{
    private readonly PlcBaseRoot _db;

    // Соответствие категорий сигналов → категории модулей в базе
    private static readonly Dictionary<string, string> SignalToModuleCategory = new()
    {
        ["DI"] = "DI",
        ["DO"] = "DO",
        ["AI"] = "AI",
        ["AO"] = "AO",
    };

    // Сроки поставки по вендорам (недели)
    private static readonly Dictionary<string, string> DeliveryTimes = new()
    {
        ["REGUL"] = "4–8 недель",
        ["ОВЕН"] = "1–2 недели",
        ["АБАК"] = "2–4 недели",
    };

    public UniversalCalculationService(PlcBaseRoot db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <summary>
    /// Рассчитать систему для указанного вендора.
    /// </summary>
    /// <param name="vendorName">Имя вендора: REGUL, ОВЕН, АБАК</param>
    /// <param name="signals">Список сигналов с количеством</param>
    /// <param name="cabinetWidthMm">Ширина шкафа в мм</param>
    /// <returns>Результат расчёта для ТКП-сравнения</returns>
    public PlcComparisonResult CalculateSystem(
        string vendorName,
        List<IoSignalRow> signals,
        double cabinetWidthMm)
    {
        // 1. Проверяем, есть ли вендор в базе
        var vendor = _db.Vendors?.FirstOrDefault(v =>
            v.Name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));

        if (vendor == null)
            return CreateError(vendorName, "Вендор не найден в базе");

        var series = vendor.Series?.FirstOrDefault();
        if (series == null)
            return CreateError(vendorName, "Нет данных о серии ПЛК");

        // 2. Получаем все модули этого вендора
        var modules = _db.Components
            .Where(c => c.Vendor.Equals(vendorName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!modules.Any())
            return CreateError(vendorName, "Нет модулей в базе");

        // 3. Ищем CPU
        var cpu = modules.FirstOrDefault(m =>
            m.Category.Equals("cpu", StringComparison.OrdinalIgnoreCase));

        if (cpu == null)
            return CreateError(vendorName, "ЦПУ не найден в базе");

        // 4. Подбираем модули под каждый тип сигнала
        int totalModules = 1; // Начинаем с CPU
        decimal totalCost = 0;
        var selectedModules = new List<(PlcComponentDto Module, int Count)>();

        foreach (var signal in signals)
        {
            if (signal.TotalWithReserve <= 0) continue;

            // Определяем категорию модуля под тип сигнала
            if (!SignalToModuleCategory.TryGetValue(signal.SignalType, out var category))
                continue;

            // Ищем модули этой категории, сортируем по убыванию каналов
            var matchingModules = modules
                .Where(m => m.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                         && m.Channels > 0)
                .OrderByDescending(m => m.Channels)
                .ToList();

            if (!matchingModules.Any()) continue;

            // Берём самый ёмкий модуль
            var bestModule = matchingModules.First();
            int modulesNeeded = (int)Math.Ceiling((double)signal.TotalWithReserve / bestModule.Channels);

            totalModules += modulesNeeded;
            selectedModules.Add((bestModule, modulesNeeded));
        }

        // 5. Считаем крейты
        int maxPerRack = series.MaxModulesPerRack > 0 ? series.MaxModulesPerRack : 12;
        int racksCount = (int)Math.Ceiling((double)totalModules / maxPerRack);

        // 6. Считаем шкафы
        // Упрощённо: полезная высота панели = высота шкафа (2000 мм) – 200 мм (короба и кабельный ввод) = 1800 мм
        // Высота одного крейта с модулями ≈ 250 мм (модули + шасси + короб)
        const double rackHeightMm = 250.0;
        double usefulPanelHeightMm = cabinetWidthMm >= 800 ? 1800 : 1400;
        int racksPerCabinet = (int)Math.Floor(usefulPanelHeightMm / rackHeightMm);
        if (racksPerCabinet < 1) racksPerCabinet = 1;

        int cabinetsCount = (int)Math.Ceiling((double)racksCount / racksPerCabinet);

        // 7. Считаем стоимость (заглушки — замените на API ЭТМ)
        totalCost = EstimateCost(vendorName, cpu, selectedModules, racksCount);

        return new PlcComparisonResult
        {
            VendorName = $"{vendor.Name} {series.Id}",
            SeriesId = series.Id,
            TargetApplication = series.TargetApplication ?? "Общепромышленная автоматизация",
            TotalHardwareCostRub = totalCost,
            TotalRacksCount = racksCount,
            TotalCabinetsCount = cabinetsCount,
            DeliveryTime = DeliveryTimes.GetValueOrDefault(vendorName, "Уточняется"),
        };
    }

    /// <summary>
    /// Оценка стоимости (заглушка до интеграции API ЭТМ).
    /// </summary>
    private decimal EstimateCost(
        string vendor,
        PlcComponentDto cpu,
        List<(PlcComponentDto Module, int Count)> selectedModules,
        int racksCount)
    {
        decimal total = 0;

        // CPU
        total += vendor switch
        {
            "REGUL" => 185000m,
            "ОВЕН" => 45000m,
            "АБАК" => 65000m,
            _ => 50000m,
        };

        // Модули
        foreach (var (module, count) in selectedModules)
        {
            decimal unitPrice = module.Category?.ToUpper() switch
            {
                "DI" => vendor switch
                {
                    "REGUL" => module.Channels >= 32 ? 38000m : 28000m,
                    "ОВЕН" => 12000m,
                    "АБАК" => 15000m,
                    _ => 10000m,
                },
                "DO" => vendor switch
                {
                    "REGUL" => module.Channels >= 32 ? 35000m : 25000m,
                    "ОВЕН" => 14000m,
                    "АБАК" => 17000m,
                    _ => 12000m,
                },
                "AI" => vendor switch
                {
                    "REGUL" => module.Channels >= 8 ? 42000m : 28000m,
                    "ОВЕН" => 18000m,
                    "АБАК" => 22000m,
                    _ => 15000m,
                },
                "AO" => vendor switch
                {
                    "REGUL" => 32000m,
                    "ОВЕН" => 16000m,
                    "АБАК" => 20000m,
                    _ => 14000m,
                },
                _ => 10000m,
            };

            total += unitPrice * count;
        }

        // Крейты и шасси
        total += vendor switch
        {
            "REGUL" => 85000m * racksCount,
            _ => 30000m * racksCount,
        };

        return total;
    }

    /// <summary>
    /// Создать результат с ошибкой.
    /// </summary>
    private static PlcComparisonResult CreateError(string vendor, string error)
    {
        return new PlcComparisonResult
        {
            VendorName = vendor,
            SeriesId = "Ошибка",
            TargetApplication = error,
            TotalHardwareCostRub = 0,
            TotalRacksCount = 0,
            TotalCabinetsCount = 0,
            DeliveryTime = "—",
        };
    }

    /// <summary>
    /// Получить список всех вендоров из базы.
    /// </summary>
    public List<string> GetAvailableVendors()
    {
        return _db.Vendors?
            .Where(v => _db.Components.Any(c =>
                c.Vendor.Equals(v.Name, StringComparison.OrdinalIgnoreCase)))
            .Select(v => v.Name)
            .ToList() ?? new List<string>();
    }
}