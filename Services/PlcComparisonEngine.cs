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
    public string DeliveryTime { get; set; } = "";
}

public class PlcComparisonEngine
{
    private readonly UniversalCalculationService _universalService;

    public PlcComparisonEngine(UniversalCalculationService universalService)
    {
        _universalService = universalService;
    }

    public Dictionary<string, PlcComparisonResult> CompareSystems(
    SignalRequirement signals,      // ← было List<IoSignalRow> inputSignals
    double cabinetWidth,
    PlcBaseRoot plcDbRoot,
    List<string>? vendorFilter = null)
    {
        var report = new Dictionary<string, PlcComparisonResult>();
        var vendorsToCompare = vendorFilter ?? _universalService.GetAvailableVendors();

        foreach (var vendor in vendorsToCompare)
        {
            try
            {
                var (result, _, _) = _universalService.CalculateSystemWithDiagnostics(vendor, signals, cabinetWidth);
                report[vendor] = result;
            }
            catch (Exception ex)
            {
                report[vendor] = new PlcComparisonResult
                {
                    VendorName = vendor,
                    SeriesId = "Ошибка",
                    TargetApplication = ex.Message,
                };
            }
        }

        return report;
    }
}
