namespace AsuGenerator.Web.Services;

public class VentAvtomatikaConfig
{
    public string ClientName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string KpNumber { get; set; } = string.Empty;

    public double SupplyFanPowerKw { get; set; }
    public string SupplyFanRegulation { get; set; } = "Прямой пуск";
    public string Heater1Type { get; set; } = "Нет";
    public double Heater1PumpPowerKw { get; set; }
}
