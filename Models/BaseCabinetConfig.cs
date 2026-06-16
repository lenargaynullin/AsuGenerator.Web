namespace AsuGenerator.Web.Models;

public class BaseCabinetConfig
{
    // Конструктивные
    public string MountType { get; set; } = "Навесной";
    private string _ipRating = "IP54";
    public string IpRating { get; set; } = "IP54";
    public bool HasPlinth { get; set; }
    public string PowerCategory { get; set; } = "III";
    public string EarthingSystem { get; set; } = "TN-S";
    public string ClimateExecution { get; set; } = "УХЛ4";
    public bool HasVentilation { get; set; } = false;
    public bool HasLighting { get; set; } = false;
    public string CableEntryDirection { get; set; } = "Bottom";
    public string GlandType { get; set; } = "Пластик";
    public int GlandsCount { get; set; } = 0;
    public string RalColor { get; set; } = "7035";

    public bool HasPocket { get; set; } = false;
    public bool HasSidePanels { get; set; } = false;
    public bool HasDoorHandle { get; set; } = false;
    public bool HasShelf { get; set; } = false;
    public string PlinthHeight { get; set; } = "100 мм";
    public string FanModel { get; set; } = "FA 12.230";
    public string Manufacturer { get; set; } = "ПРОВЕНТО";

    // Оборудование
    public string TerminalVendor { get; set; } = "Klemsan";
    public int TerminalReservePercent { get; set; } = 10;
    public string DinRailType { get; set; } = "Оцинкованная";
    public string TrunkingSize { get; set; } = "60x40";
    public string TrunkingMaterial { get; set; } = "ПВХ";
    public bool AutoCalculateTrunking { get; set; } = true;
    public bool IncludeWireAndFerrules { get; set; } = false;

    // Событие для сброса габаритов
    public event Action<string>? OnIpRatingChanged;
    public int Height { get; set; } = 600;
    public int Width { get; set; } = 400;
    public int Depth { get; set; } = 250;
    public string Material { get; set; } = "Металл";
    public bool HasHeater { get; set; } = false;

    // Электрические
    public string Voltage { get; set; } = "380";
    public int Phases { get; set; } = 3;
    public int InputCount { get; set; } = 1;
    public string InputType { get; set; } = "Кабельный";
    public int InputCurrent { get; set; } = 32;
    public string GroundSystem { get; set; } = "TN-S";
    public int ShortCircuitCurrent { get; set; } = 6;

    // Системные
    public string PlcType { get; set; } = "ПР200";
    public bool HasDisplay { get; set; } = false;
    public string Protocol { get; set; } = "Modbus RTU";
    public bool HasDispatching { get; set; } = false;
    public string ControlMode { get; set; } = "Местное";

    // Коммерческие
    public string ProjectName { get; set; } = "";
    public string ProjectNumber { get; set; } = "";
    public string ClientName { get; set; } = "";
    public decimal Margin { get; set; } = 1.35m;
    public string PreferredBrand { get; set; } = "KEAZ"; // КЭАЗ / EKF / IEK / Chint
}