using System;
using System.Collections.Generic;

namespace AsuGenerator.Web.Models;

public enum RegulPowerType { OneBus, TwoBus }

public class RegulModuleInfo
{
    public string PartNumber { get; set; } = "";
    public string Description { get; set; } = "";
    public double WidthMm { get; set; }
    public double CurrentConsumptionA { get; set; }
    public RegulPowerType PowerType { get; set; }
    public bool IsIoOrCpu { get; set; }
}

public class AssociatedAccessory
{
    public string PartNumber { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public string Type { get; set; } = ""; // "Барьер" или "Реле"
    public double WidthMm { get; set; }
}

public class RegulRackResult
{
    public int RackIndex { get; set; }
    public double CalculatedBusCurrentA { get; set; }
    public double TotalRackWidthMm { get; set; }
    public double TotalAccessoriesWidthMm { get; set; }
    public List<CustomTemplateItem> AddedComponents { get; set; } = new();
    public List<AssociatedAccessory> Accessories { get; set; } = new();
}

public class IoSignalRow
{
    public string SignalType { get; set; } = "";
    public int BaseCount { get; set; }
    public int TotalWithReserve { get; set; }
}

public class RegulCabinetResult
{
    public int CabinetIndex { get; set; }
    public double EnclosureWidthMm { get; set; }
    public double UsefulWidthMm => EnclosureWidthMm - 100;
    public List<RegulRackResult> RacksInCabinet { get; set; } = new();
}
