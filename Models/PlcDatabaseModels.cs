using System.Collections.Generic;

namespace AsuGenerator.Web.Models;

public class PlcBaseRoot
{
    public GlobalPlcSettings GlobalSettings { get; set; } = new();
    public List<PlcVendorDto> Vendors { get; set; } = new();
    public List<PlcComponentDto> Components { get; set; } = new();
    public List<PlcChassisDto> Chassis { get; set; } = new();
    public List<PlcBarrierDto> Barriers { get; set; } = new();
}

public class GlobalPlcSettings
{
    public int ReserveAiPercent { get; set; }
    public int ReserveDiPercent { get; set; }
    public int DefaultCabinetWidthMm { get; set; }
    public double MaxBusCurrentA { get; set; }
}

public class PlcVendorDto
{
    public string Name { get; set; } = "";
    public List<PlcSeriesDto> Series { get; set; } = new();
}

public class PlcSeriesDto
{
    public string Id { get; set; } = "";
    public int MaxModulesPerRack { get; set; }
    public decimal RackPriceRub { get; set; }
    public string TargetApplication { get; set; } = "";
}

public class PlcComponentDto
{
    public string Vendor { get; set; } = "";
    public string FamilyId { get; set; } = "";
    public string PartNumber { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public int Channels { get; set; }
    public double WidthMm { get; set; }
    public double HeightMm { get; set; }
    public double PowerConsumptionW { get; set; }
    public bool TwoBusSupport { get; set; }
    public string CompatibleChassis { get; set; } = "";
}

public class PlcChassisDto
{
    public string PartNumber { get; set; } = "";
    public double WidthMm { get; set; }
    public string Description { get; set; } = "";
}

public class PlcBarrierDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public int ChannelsPerBarrier { get; set; }
    public string Description { get; set; } = "";
}
