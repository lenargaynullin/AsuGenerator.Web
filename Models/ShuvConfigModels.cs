using System.Collections.Generic;

namespace AsuGenerator.Web.Models;

public class ShuvConfig
{
    public string CabinetType { get; set; } = "";
    public string Description { get; set; } = "";
    public List<DeviceConfig> Devices { get; set; } = [];
}

public class DeviceConfig
{
    public string Key { get; set; } = "";
    public string Designation { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Vendor { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public DeviceParamsConfig? Params { get; set; }
    public DeviceCondition? Condition { get; set; }
}

public class DeviceCondition
{
    public string Field { get; set; } = "";
    public string Value { get; set; } = "";
}

public class DeviceParamsConfig
{
    public int Poles { get; set; }
    public double RatedCurrent { get; set; }
    public double Current { get; set; }
    public string Type { get; set; } = "";
    public int Voltage { get; set; }
    public int Power { get; set; }
    public string Model { get; set; } = "";
    public string Color { get; set; } = "";
    public int Positions { get; set; }
    public double Section { get; set; }
    public int InputVoltage { get; set; }
    public int OutputVoltage { get; set; }
    public int Di { get; set; }
    public int Do { get; set; }
    public int Ai { get; set; }
    public string Interface { get; set; } = "";
    public string Contacts { get; set; } = "";
}