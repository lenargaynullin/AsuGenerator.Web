using System.Collections.Generic;

namespace AsuGenerator.Web.Models;

public class ShuvConfig
{
    public string CabinetType { get; set; } = "";
    public string Version { get; set; } = "";
    public List<DeviceConfig> Rules { get; set; } = [];
}
public class DeviceConfig
{
    public string Designation { get; set; } = "";
    public string Category { get; set; } = "";
    public string TechParameters { get; set; } = "";
    public string Condition { get; set; } = "Always";
    public string DefaultSupplier { get; set; } = "";
    public string Article { get; set; } = "";
    public string Description { get; set; } = "";
    public int Quantity { get; set; } = 1;
}
public class HeatingLine
{
    public bool IsEnabled { get; set; } = false;
    public string Designation { get; set; } = "";
    public bool HasRCD { get; set; }
    public int Poles { get; set; } = 3;
    public int Current { get; set; } = 16;
    public string Curve { get; set; } = "C";
    public double IkZ { get; set; } = 6;
    public bool HasContactor { get; set; }
    public int ContactorCurrent { get; set; }
    public bool HasThermostat { get; set; }
    public bool HasAuxContact { get; set; }
}