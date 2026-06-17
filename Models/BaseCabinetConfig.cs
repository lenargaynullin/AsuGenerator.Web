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


    public bool HasEyebolts { get; set; }        // Рым-болты
    public bool HasDoorRails { get; set; }       // Рейка для глухой двери
    public bool HasRoofCablePanel { get; set; }  // Потолочная панель с вводом для кабелей
    public bool HasSupportBase { get; set; }     // Подставка
    public bool HasMountPanel { get; set; }      // Монтажная панель
    public bool HasCabinetDoor { get; set; }     // Дверь
    public bool HasLimitSwitch { get; set; }     // Концевой выключатель


    // Оборудование
    public string TerminalVendor { get; set; } = "STEZ";
    public int TerminalReservePercent { get; set; } = 10;
    public string DinRailType { get; set; } = "Оцинкованная";
    public string TrunkingSize { get; set; } = "60x40";
    public string TrunkingMaterial { get; set; } = "ПВХ";
    public bool AutoCalculateTrunking { get; set; } = true;
    public bool IncludeWireAndFerrules { get; set; } = false;
    public int FanQuantity { get; set; }


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

    // PLC
    public string PlcPower { get; set; } = "220";      // "220" (~230В) или "24" (=24В)
    public string PlcDiType { get; set; } = "Стандарт"; // "Стандарт" (~230В) или "24В" для дискретных входов
    public string PlcInterfaces { get; set; } = "2";    // "0" (без), "1" (один RS-485), "2" (два RS-485)
    public int AiCount { get; set; } = 0;
    public int AoCount { get; set; } = 0;
    public int DiCount { get; set; } = 0;
    public int DoCount { get; set; } = 0;
    public string AoType { get; set; } = "";

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
    public string PreferredBrand { get; set; } = "КЭАЗ"; // КЭАЗ / EKF / IEK / Chint
}