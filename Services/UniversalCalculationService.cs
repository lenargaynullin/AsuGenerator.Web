using AsuGenerator.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Services;

public class UniversalCalculationService
{
    private readonly PlcBaseRoot _db;

    // Маппинг 25 типов сигналов на категории модулей
    private static readonly Dictionary<string, string> SignalToCategory = new(StringComparer.OrdinalIgnoreCase)
    {
        // Аналоговые входы (AI)
        ["AI_NIS_2W"] = "ai",
        ["AI_NIS_4W"] = "ai",
        ["AI_RTD_NIS_3W"] = "ai-rtd",
        ["AI_NIS_PL_3W"] = "ai",
        ["AI_IS_2W"] = "ai",
        ["AI_IS_4W"] = "ai",
        ["AI_IS_PL_3W"] = "ai",
        ["AI_R_NIS_2W"] = "ai",
        ["AI_R_NIS_4W"] = "ai",
        ["AI_R_IS_2W"] = "ai",
        ["AI_R_IS_4W"] = "ai",

        // Аналоговые выходы (AO)
        ["AO_NIS"] = "ao",
        ["AO_NIS_V"] = "ao",
        ["AO_IS"] = "ao",
        ["AO_IS_V"] = "ao",
        ["AO_R_IS"] = "ao",
        ["AO_R_NIS"] = "ao",

        // Дискретные входы (DI)
        ["DI_IS_NAMUR"] = "di",
        ["DI_NIS_DRY"] = "di",
        ["DI_NIS_24V"] = "di",
        ["DI_NIS_VFG"] = "di",
        ["DI_NIS_MCC_230VAC"] = "di",
        ["DI_NIS_MCC_220VDC"] = "di",

        // Дискретные выходы (DO)
        ["DO_NIS_VFC"] = "do",
        ["DO_NIS_24V"] = "do",
        ["DO_NIS_MCC_230VAC"] = "do",
        ["DO_NIS_MCC_220VDC"] = "do",
    };

    // Сроки поставки
    private static readonly Dictionary<string, string> DeliveryTimes = new(StringComparer.OrdinalIgnoreCase)
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
    /// Принимает SignalRequirement с 25 типами сигналов.
    /// </summary>
    public PlcComparisonResult CalculateSystem(
        string vendorName,
        SignalRequirement signals,
        double cabinetWidthMm)
    {
        var vendor = _db.Vendors?.FirstOrDefault(v =>
            v.Name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));

        if (vendor == null)
            return CreateError(vendorName, "Вендор не найден в базе");

        var series = vendor.Series?.FirstOrDefault();
        if (series == null)
            return CreateError(vendorName, "Нет данных о серии ПЛК");

        var modules = _db.Components
            .Where(c => c.Vendor != null &&
                        c.Vendor.Equals(vendorName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!modules.Any())
            return CreateError(vendorName, "Нет модулей в базе");

        var cpu = modules.FirstOrDefault(m =>
            m.Category != null && m.Category.Equals("cpu", StringComparison.OrdinalIgnoreCase));

        if (cpu == null)
            return CreateError(vendorName, "ЦПУ не найден в базе");

        // Собираем все сигналы в плоский список с ключами типа
        var signalList = new List<(string Key, int Count)>
        {
            // AI
            ("AI_NIS_2W", signals.AiNisCurrent2W),
            ("AI_NIS_4W", signals.AiNisCurrent4W),
            ("AI_RTD_NIS_3W", signals.AiRtdNis3W),
            ("AI_NIS_PL_3W", signals.AiNisPl3W),
            ("AI_IS_2W", signals.AiIsCurrent2W),
            ("AI_IS_4W", signals.AiIsCurrent4W),
            ("AI_IS_PL_3W", signals.AiIsPl3W),
            ("AI_R_NIS_2W", signals.AiRNisCurrent2W),
            ("AI_R_NIS_4W", signals.AiRNisCurrent4W),
            ("AI_R_IS_2W", signals.AiRIsCurrent2W),
            ("AI_R_IS_4W", signals.AiRIsCurrent4W),
            // AO
            ("AO_NIS", signals.AoNisCurrent),
            ("AO_NIS_V", signals.AoNisVoltage),
            ("AO_IS", signals.AoIsCurrent),
            ("AO_IS_V", signals.AoIsVoltage),
            ("AO_R_IS", signals.AoRIsCurrent),
            ("AO_R_NIS", signals.AoRNisCurrent),
            // DI
            ("DI_IS_NAMUR", signals.DiIsNamur),
            ("DI_NIS_DRY", signals.DiNisDryContact),
            ("DI_NIS_24V", signals.DiNis24VDC),
            ("DI_NIS_VFG", signals.DiNisVfg),
            ("DI_NIS_MCC_230VAC", signals.DiNisMcc230VAC),
            ("DI_NIS_MCC_220VDC", signals.DiNisMcc220VDC),
            // DO
            ("DO_NIS_VFC", signals.DoNisVfcNO),
            ("DO_NIS_24V", signals.DoNis24VDC),
            ("DO_NIS_MCC_230VAC", signals.DoNisMcc230VAC),
            ("DO_NIS_MCC_220VDC", signals.DoNisMcc220VDC),
        };

        double reserveFactor = 1 + ((double)signals.ReservePercent / 100.0);
        int totalModules = 1; // CPU
        decimal totalCost = 0;

        foreach (var (key, count) in signalList)
        {
            if (count <= 0) continue;

            if (!SignalToCategory.TryGetValue(key, out var category))
                continue;

            int countWithReserve = (int)Math.Ceiling(count * reserveFactor);

            // Базовый фильтр: категория + есть каналы
            var matchingModules = modules
                .Where(m => m.Category != null &&
                            m.Category.Equals(category, StringComparison.OrdinalIgnoreCase) &&
                            m.Channels > 0);

            // Дополнительный фильтр по типу сигнала
            matchingModules = matchingModules.Where(m => IsModuleCompatible(key, m));

            var bestModule = matchingModules
                .OrderByDescending(m => m.Channels)
                .FirstOrDefault();

            if (bestModule == null) continue;

            int modulesNeeded = (int)Math.Ceiling((double)countWithReserve / bestModule.Channels);
            totalModules += modulesNeeded;
        }

        // Крейты
        int maxPerRack = series.MaxModulesPerRack > 0 ? series.MaxModulesPerRack : 40;
        int racksCount = (int)Math.Ceiling((double)totalModules / maxPerRack);

        // Шкафы (упрощённо)
        int racksPerCabinet = cabinetWidthMm >= 800 ? 2 : 1;
        int cabinetsCount = (int)Math.Ceiling((double)racksCount / racksPerCabinet);

        // Стоимость
        totalCost = EstimateCost(vendorName, totalModules, racksCount);

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
    /// Проверяет, совместим ли модуль с конкретным типом сигнала.
    /// Анализирует Description и PartNumber модуля.
    /// </summary>
    private bool IsModuleCompatible(string signalKey, PlcComponentDto module)
    {
        var desc = (module.Description ?? "").ToLower();
        var part = (module.PartNumber ?? "").ToLower();

        return signalKey switch
        {
            // AI токовые — в описании должны быть "4" и "20" (4-20 мА)
            "AI_NIS_2W" or "AI_NIS_4W" or "AI_IS_2W" or "AI_IS_4W"
                or "AI_R_NIS_2W" or "AI_R_NIS_4W" or "AI_R_IS_2W" or "AI_R_IS_4W"
                => desc.Contains("4") && desc.Contains("20"),

            // AI RTD — термосопротивления
            "AI_RTD_NIS_3W"
                => desc.Contains("термосопротивлени") || desc.Contains("rtd") || desc.Contains("pt100"),

            // AI термопары
            "AI_TC"
                => desc.Contains("термопар") || desc.Contains("tc"),

            // AI частота
            "AI_NIS_PL_3W" or "AI_IS_PL_3W"
                => desc.Contains("частот") || desc.Contains("импульс") || desc.Contains("кГц"),

            // AO токовые
            "AO_NIS" or "AO_IS" or "AO_R_IS" or "AO_R_NIS"
                => desc.Contains("4") && desc.Contains("20") && desc.Contains("вывод"),

            // AO напряжение
            "AO_NIS_V" or "AO_IS_V"
                => desc.Contains("0") && desc.Contains("10") && desc.Contains("в"),

            // DI NAMUR
            "DI_IS_NAMUR"
                => desc.Contains("namur"),

            // DI сухой контакт
            "DI_NIS_DRY"
                => desc.Contains("сухой") || desc.Contains("контакт"),

            // DI 24V
            "DI_NIS_24V"
                => desc.Contains("24") && desc.Contains("в") && desc.Contains("ввод"),

            // DI VFG (частотный)
            "DI_NIS_VFG"
                => desc.Contains("частот") || desc.Contains("импульс") || desc.Contains("vfg"),

            // DI MCC 230VAC
            "DI_NIS_MCC_230VAC"
                => desc.Contains("230") || desc.Contains("220") && desc.Contains("ac"),

            // DI MCC 220VDC
            "DI_NIS_MCC_220VDC"
                => desc.Contains("220") && desc.Contains("dc"),

            // DO VFC
            "DO_NIS_VFC"
                => desc.Contains("vfc") || desc.Contains("сухой"),

            // DO 24V
            "DO_NIS_24V"
                => desc.Contains("24") && desc.Contains("вывод"),

            // DO MCC
            "DO_NIS_MCC_230VAC"
                => desc.Contains("230") || desc.Contains("220") && desc.Contains("ac") && desc.Contains("вывод"),
            "DO_NIS_MCC_220VDC"
                => desc.Contains("220") && desc.Contains("dc") && desc.Contains("вывод"),

            // По умолчанию — подходит
            _ => true,
        };
    }
    private decimal EstimateCost(string vendor, int totalModules, int racksCount)
    {
        decimal modulePrice = vendor switch
        {
            "REGUL" => 30000m,
            "АБАК" => 15000m,
            "ОВЕН" => 12000m,
            _ => 10000m,
        };

        decimal rackPrice = vendor switch
        {
            "REGUL" => 85000m,
            "АБАК" => 45000m,
            "ОВЕН" => 30000m,
            _ => 25000m,
        };

        decimal cpuPrice = vendor switch
        {
            "REGUL" => 185000m,
            "АБАК" => 65000m,
            "ОВЕН" => 45000m,
            _ => 50000m,
        };

        return cpuPrice + (modulePrice * totalModules) + (rackPrice * racksCount);
    }

    public List<string> GetAvailableVendors()
    {
        return _db.Vendors?
            .Where(v => _db.Components.Any(c =>
                c.Vendor != null &&
                c.Vendor.Equals(v.Name, StringComparison.OrdinalIgnoreCase)))
            .Select(v => v.Name)
            .ToList() ?? new List<string>();
    }

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
    /// Рассчитать систему с диагностикой — возвращает и результат, и лог подбора.
    /// </summary>
    public (PlcComparisonResult Result, List<string> Log) CalculateSystemWithDiagnostics(
        string vendorName,
        SignalRequirement signals,
        double cabinetWidthMm)
    {
        var log = new List<string>();

        var vendor = _db.Vendors?.FirstOrDefault(v =>
            v.Name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));

        if (vendor == null)
            return (CreateError(vendorName, "Вендор не найден в базе"), log);

        var series = vendor.Series?.FirstOrDefault();
        if (series == null)
            return (CreateError(vendorName, "Нет данных о серии ПЛК"), log);

        var modules = _db.Components
            .Where(c => c.Vendor != null &&
                        c.Vendor.Equals(vendorName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        log.Add($"🔍 Вендор: {vendorName}");
        log.Add($"📦 Всего модулей в базе: {modules.Count} шт.");
        log.Add("");

        if (!modules.Any())
            return (CreateError(vendorName, "Нет модулей в базе"), log);

        var cpu = modules.FirstOrDefault(m =>
            m.Category != null && m.Category.Equals("cpu", StringComparison.OrdinalIgnoreCase));

        if (cpu == null)
        {
            log.Add("❌ CPU не найден!");
            return (CreateError(vendorName, "ЦПУ не найден в базе"), log);
        }

        log.Add($"✅ CPU: {cpu.PartNumber}");

        var signalList = new List<(string Key, string Label, int Count)>
    {
        ("AI_NIS_2W", "AI-NIS (4-20 mA, 2w)", signals.AiNisCurrent2W),
        ("AI_NIS_4W", "AI-NIS (4-20 mA, 4w)", signals.AiNisCurrent4W),
        ("AI_RTD_NIS_3W", "AI-RTD-NIS (3w)", signals.AiRtdNis3W),
        ("AI_NIS_PL_3W", "AI-NIS (Pl, 3w)", signals.AiNisPl3W),
        ("AI_IS_2W", "AI-IS (4-20 mA, 2w)", signals.AiIsCurrent2W),
        ("AI_IS_4W", "AI-IS (4-20 mA, 4w)", signals.AiIsCurrent4W),
        ("AI_IS_PL_3W", "AI-IS (Pl, 3w)", signals.AiIsPl3W),
        ("AI_R_NIS_2W", "AI-R-NIS (4-20 mA, 2w)", signals.AiRNisCurrent2W),
        ("AI_R_NIS_4W", "AI-R-NIS (4-20 mA, 4w)", signals.AiRNisCurrent4W),
        ("AI_R_IS_2W", "AI-R-IS (4-20 mA, 2w)", signals.AiRIsCurrent2W),
        ("AI_R_IS_4W", "AI-R-IS (4-20 mA, 4w)", signals.AiRIsCurrent4W),
        ("AO_NIS", "AO-NIS (4-20 mA)", signals.AoNisCurrent),
        ("AO_NIS_V", "AO-NIS (0-10 В)", signals.AoNisVoltage),
        ("AO_IS", "AO-IS (4-20 mA)", signals.AoIsCurrent),
        ("AO_IS_V", "AO-IS (0-10 В)", signals.AoIsVoltage),
        ("AO_R_IS", "AO-R-IS (4-20 mA)", signals.AoRIsCurrent),
        ("AO_R_NIS", "AO-R-NIS (4-20 mA)", signals.AoRNisCurrent),
        ("DI_IS_NAMUR", "DI-IS (NAMUR)", signals.DiIsNamur),
        ("DI_NIS_DRY", "DI-NIS (сухой контакт)", signals.DiNisDryContact),
        ("DI_NIS_24V", "DI-NIS (24VDC)", signals.DiNis24VDC),
        ("DI_NIS_VFG", "DI-NIS (VFG)", signals.DiNisVfg),
        ("DI_NIS_MCC_230VAC", "DI-NIS (MCC 230VAC)", signals.DiNisMcc230VAC),
        ("DI_NIS_MCC_220VDC", "DI-NIS (MCC 220VDC)", signals.DiNisMcc220VDC),
        ("DO_NIS_VFC", "DO-NIS (VFC NO)", signals.DoNisVfcNO),
        ("DO_NIS_24V", "DO-NIS (24VDC)", signals.DoNis24VDC),
        ("DO_NIS_MCC_230VAC", "DO-NIS (MCC 230VAC)", signals.DoNisMcc230VAC),
        ("DO_NIS_MCC_220VDC", "DO-NIS (MCC 220VDC)", signals.DoNisMcc220VDC),
    };

        double reserveFactor = 1 + ((double)signals.ReservePercent / 100.0);
        int totalModules = 1; // CPU

        foreach (var (key, label, count) in signalList)
        {
            if (count <= 0) continue;

            if (!SignalToCategory.TryGetValue(key, out var category))
            {
                log.Add($"⚠️ {label}: {count} шт. → категория не найдена в словаре");
                continue;
            }

            int countWithReserve = (int)Math.Ceiling(count * reserveFactor);

            // Все модули этой категории с каналами
            var allCategoryModules = modules
                .Where(m => m.Category != null &&
                            m.Category.Equals(category, StringComparison.OrdinalIgnoreCase) &&
                            m.Channels > 0)
                .ToList();

            log.Add($"🔎 {label}: {count} шт. (с резервом: {countWithReserve}) → категория '{category}'. Найдено модулей в категории: {allCategoryModules.Count}");

            // Фильтруем по совместимости описания
            var compatibleModules = allCategoryModules
                .Where(m => IsModuleCompatible(key, m))
                .OrderByDescending(m => m.Channels)
                .ToList();

            if (!compatibleModules.Any())
            {
                log.Add($"❌ {label}: из {allCategoryModules.Count} модулей категории '{category}' ни один не совместим по описанию!");
                if (allCategoryModules.Any())
                {
                    log.Add($"   Примеры модулей в категории: {string.Join(", ", allCategoryModules.Take(3).Select(m => $"{m.PartNumber} (desc: '{m.Description?.Substring(0, Math.Min(m.Description?.Length ?? 0, 60))}...')"))}");
                }
                continue;
            }

            var bestModule = compatibleModules.First();
            int modulesNeeded = (int)Math.Ceiling((double)countWithReserve / bestModule.Channels);
            totalModules += modulesNeeded;

            log.Add($"✅ {label}: {modulesNeeded} × {bestModule.PartNumber} (каналов: {bestModule.Channels}, ширина: {bestModule.WidthMm}мм)");
        }

        int maxPerRack = series.MaxModulesPerRack > 0 ? series.MaxModulesPerRack : 40;
        int racksCount = (int)Math.Ceiling((double)totalModules / maxPerRack);
        int racksPerCabinet = cabinetWidthMm >= 800 ? 2 : 1;
        int cabinetsCount = (int)Math.Ceiling((double)racksCount / racksPerCabinet);

        log.Add("");
        log.Add($"📊 ИТОГО: модулей={totalModules}, крейтов={racksCount}, шкафов={cabinetsCount}");
        log.Add($"💰 Стоимость: {EstimateCost(vendorName, totalModules, racksCount):N0} ₽");

        var result = new PlcComparisonResult
        {
            VendorName = $"{vendor.Name} {series.Id}",
            SeriesId = series.Id,
            TargetApplication = series.TargetApplication ?? "Общепромышленная автоматизация",
            TotalHardwareCostRub = EstimateCost(vendorName, totalModules, racksCount),
            TotalRacksCount = racksCount,
            TotalCabinetsCount = cabinetsCount,
            DeliveryTime = DeliveryTimes.GetValueOrDefault(vendorName, "Уточняется"),
        };

        return (result, log);
    }
}