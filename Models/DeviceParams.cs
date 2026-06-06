namespace AsuGenerator.Web.Models;

/// <summary>
/// Параметры автоматического выключателя.
/// Храним не артикул, а характеристики — артикул подбирается под бренд.
/// </summary>
public class BreakerParams
{
    public int Poles { get; set; } = 3;           // 1, 2, 3, 4
    public double RatedCurrent { get; set; } = 18; // 18 А
    public string Curve { get; set; } = "C";      // B, C, D
    public int IkZ { get; set; } = 6;             // 6 кА, 10 кА
    public string Type { get; set; } = "Motor";   // "Motor" (защита двигателя), "Standard"
}

/// <summary>
/// Параметры контактора.
/// </summary>
public class ContactorParams
{
    public int Poles { get; set; } = 3;
    public double RatedCurrent { get; set; } = 16;
    public string CoilVoltage { get; set; } = "230V";
}

/// <summary>
/// Шаблон устройства в проекте.
/// Описывает, ЧТО нужно, но не привязан к конкретному артикулу.
/// </summary>
public class DeviceTemplate
{
    public string Designation { get; set; } = "";       // "QF1"
    public string DeviceType { get; set; } = "";        // "Breaker", "Contactor", "PLC", "Relay"
    public object Params { get; set; } = new();         // BreakerParams или ContactorParams
    public int Quantity { get; set; } = 1;
}