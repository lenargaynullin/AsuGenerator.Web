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