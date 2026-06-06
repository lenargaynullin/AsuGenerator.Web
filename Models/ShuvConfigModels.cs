using System.Collections.Generic;

namespace AsuGenerator.Web.Models;

public class ShuvConfig
{
    public string CabinetType { get; set; } = "";
    public string Description { get; set; } = "";
    public Dictionary<string, Dictionary<string, List<string>>> Rules { get; set; } = new();
    public List<string> CommonDevices { get; set; } = new();
    public Dictionary<string, DeviceConfig> Devices { get; set; } = new();
}

public class DeviceConfig
{
    public string Designation { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Vendor { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public DeviceParamsConfig? Params { get; set; }
}

public class DeviceParamsConfig
{
    public int Poles { get; set; }
    public double RatedCurrent { get; set; }
    public string Type { get; set; } = "";
    public int Voltage { get; set; }
    public int Power { get; set; }
    public string Model { get; set; } = "";
    public string Color { get; set; } = "";
    public int Positions { get; set; }
    public double Section { get; set; }
    public int Current { get; set; }
}