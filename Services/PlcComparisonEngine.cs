using System;
using System.Collections.Generic;
using System.Linq;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

public class PlcComparisonResult
{
    public string VendorName { get; set; } = "";
    public string SeriesId { get; set; } = "";
    public int TotalCabinetsCount { get; set; }
    public int TotalRacksCount { get; set; }
    public decimal TotalHardwareCostRub { get; set; }
    public string TargetApplication { get; set; } = "";
}

public class PlcComparisonEngine
{
    private readonly RegulCalculationService _regulService;

    // Внедряем через конструктор наш боевой сервис REGUL, чтобы использовать его алгоритмы
    public PlcComparisonEngine(RegulCalculationService regulService)
    {
        _regulService = regulService;
    }

    public Dictionary<string, PlcComparisonResult> CompareSystems(List<IoSignalRow> signals, double selectedWidthMm, PlcBaseRoot plcDb)
    {
        var report = new Dictionary<string, PlcComparisonResult>();
        if (signals == null || !signals.Any()) return report;

        // --- 1. РАСЧЕТ ДЛЯ СИСТЕМЫ REGUL R500 (ПРОСОФТ) ---
        var regulRacks = _regulService.CalculateSystemWithDbAndBarriers(signals);
        var regulCabinets = _regulService.DistributeRacksToCabinets(regulRacks, selectedWidthMm);

        decimal regulCost = 0;
        foreach (var rack in regulRacks)
        {
            foreach (var c in rack.AddedComponents)
            {
                // Накапливаем b2b себестоимость по артикулам Прософт
                decimal price = c.PartNumber.Contains("CU") ? 85000 :
                               c.PartNumber.Contains("PP") ? 25000 :
                               c.PartNumber.Contains("CH") ? 8000 : 2100;
                regulCost += price * (decimal)c.Quantity;
            }
            // Добавляем стоимость барьеров искрозащиты Exi
            foreach (var acc in rack.Accessories)
            {
                regulCost += 4200 * acc.Quantity;
            }
        }

        var regulMeta = plcDb?.Vendors.FirstOrDefault(v => v.Name == "REGUL")?.Series.FirstOrDefault(s => s.Id == "R500");

        report["REGUL"] = new PlcComparisonResult
        {
            VendorName = "REGUL (Прософт-Систем)",
            SeriesId = "R500",
            TotalRacksCount = regulRacks.Count,
            TotalCabinetsCount = regulCabinets.Count,
            TotalHardwareCostRub = regulCost,
            TargetApplication = regulMeta?.TargetApplication ?? "РСУ/ПАЗ до SIL3"
        };

        // --- 2. РАСЧЕТ ДЛЯ СИСТЕМЫ ОВЕН (ПЛК210) ---
        int totalDi = signals.Where(s => s.SignalType.Contains("DI")).Sum(s => s.TotalWithReserve);
        int totalDo = signals.Where(s => s.SignalType.Contains("DO")).Sum(s => s.TotalWithReserve);
        int totalAi = signals.Where(s => s.SignalType.Contains("AI")).Sum(s => s.TotalWithReserve);

        // Расчет модулей ОВЕН МВ210 (16DI / 16DO / 8AI) с учетом 20% резерва
        int diModules = (int)Math.Ceiling((double)totalDi * 1.20 / 16);
        int doModules = (int)Math.Ceiling((double)totalDo * 1.20 / 16);
        int aiModules = (int)Math.Ceiling((double)totalAi * 1.20 / 8);

        decimal ovenCost = 45000; // 1х ЦП ПЛК210-01-CS
        ovenCost += diModules * 14200; // МВ210-202
        ovenCost += doModules * 15100; // МУ210-401
        ovenCost += aiModules * 18400; // МВ210-101

        int ovenModulesTotal = diModules + doModules + aiModules;
        // Ограничение по системной шине ОВЕН: максимум 12 модулей на один ЦП
        int ovenRacksCount = (int)Math.Ceiling((double)ovenModulesTotal / 12);
        if (ovenRacksCount == 0) ovenRacksCount = 1;

        var ovenMeta = plcDb?.Vendors.FirstOrDefault(v => v.Name == "ОВЕН")?.Series.FirstOrDefault(s => s.Id == "ПЛК210");

        report["ОВЕН"] = new PlcComparisonResult
        {
            VendorName = "ОВЕН",
            SeriesId = "ПЛК210",
            TotalRacksCount = ovenRacksCount,
            TotalCabinetsCount = (int)Math.Ceiling((double)ovenRacksCount / 2), // Компактное размещение ОВЕН (по 2 узла на шкаф 800мм)
            TotalHardwareCostRub = ovenCost,
            TargetApplication = ovenMeta?.TargetApplication ?? "Общепромышленная автоматизация"
        };

        return report;
    }
}
