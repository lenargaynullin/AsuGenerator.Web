namespace AsuGenerator.Web.Services;

// 1. Модель параметров опросного листа
public class VentAvtomatikaConfig
{
    // Шапка проекта и административные данные
    public string ClientName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string KpNumber { get; set; } = string.Empty;
    public string ProjectNumber { get; set; } = string.Empty;
    public string DocDesignation { get; set; } = string.Empty;
    public string CabinetName { get; set; } = string.Empty; // Добавлено!

    // Силовые переменные фидеров обогрева ШУЭ
    public int OutletsHeatingCount { get; set; } = 5;
    public double HeaterEl1Power { get; set; } = 1.5;
    public string BreakerBrand { get; set; } = "KEAZ";
    public string EnclosureType { get; set; } = "Пластик IP41 (В помещении)";
    public string NetworkProtocol { get; set; } = "RS-485 (Modbus RTU)";
    public bool FanInReserve { get; set; }

    // Переменные для совместимости с парсером и UI (Убирают ошибки)
    public string HeaterEl1Voltage { get; set; } = "400 В/3ф"; // Добавлено!
    public string ValveInVoltage { get; set; } = "230 В";       // Добавлено!
    public bool ValveInSpring { get; set; }                     // Добавлено!
    public double SupplyFanPowerKw { get; set; }
    public string SupplyFanRegulation { get; set; } = "Прямой пуск";
    public string Heater1Type { get; set; } = "Электрический";
    public double HeaterW1PumpPower { get; set; }
}

// 2. Модель подобранного b2b-оборудования
public class SelectedComponent
{
    public string Designation { get; set; } = string.Empty; // QF1, KM1
    public string Vendor { get; set; } = string.Empty;      // КЭАЗ, ОВЕН
    public string Description { get; set; } = string.Empty; // Наименование
    public string Article { get; set; } = string.Empty;     // Артикул
    public double Current { get; set; }                     // Ток
}
// ТКП
public class CommercialProposal
{
    public string ProjectName { get; set; } = "ШУВ-1 (ПВУ с водяным нагревом)";
    public string ClientName { get; set; } = "ООО ВентАвтоматика";
    public List<ProposalItem> Items { get; set; } = new();
    public decimal AssemblyPrice { get; set; } = 45000.00m;
    public decimal TotalPrice => Items.Sum(i => i.FinalRowPrice) + AssemblyPrice;
}

public class ProposalItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal BasePrice { get; set; }
    public decimal MarginMultiplier { get; set; } = 1.35m;
    public decimal FinalUnitPrice => BasePrice * MarginMultiplier;
    public decimal FinalRowPrice => FinalUnitPrice * Quantity;
}
