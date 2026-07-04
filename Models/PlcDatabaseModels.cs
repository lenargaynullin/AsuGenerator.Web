using System.Collections.Generic;

namespace AsuGenerator.Web.Models;

public class PlcBaseRoot
{
    public List<PlcVendorDto> Vendors { get; set; } = new();
    public List<PlcChassisDto> Chassis { get; set; } = new();
    public List<PlcComponentDto> Components { get; set; } = new();
    public List<PlcBarrierDto> Barriers { get; set; } = new();
    public GlobalPlcSettings GlobalSettings { get; set; } = new();

    // ДОБАВЛЯЕМ ДВА НОВЫХ СПИСКА ДЛЯ ПОЛНОЙ СИНХРОНИЗАЦИИ
    public List<PlcMappingDto> TableMappings { get; set; } = new();
    public Dictionary<string, string> SystemDefaults { get; set; } = new();
}
// Чертеж для листа "mapping"
public class PlcMappingDto
{
    public int FamilyId { get; set; }
    public string SignalType { get; set; }
    public string RecommendedModuleId { get; set; } // Сюда запишется имя/id модуля
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
    public int Id { get; set; }
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
    public int Id { get; set; }
    public int ManufacturerId { get; set; } // ИСПРАВЛЕНО: Ссылка на производителя
    public string FamilyName { get; set; } // Имя семейства ("ПЛК АБАК K3")
    public int MaxModulesPerRack { get; set; }
}

public class PlcBarrierDto
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public int ChannelsPerBarrier { get; set; }
    public string Description { get; set; } = "";
}
