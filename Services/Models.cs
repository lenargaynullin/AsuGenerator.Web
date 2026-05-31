namespace AsuGenerator.Web.Services;

public class VentAvtomatikaConfig
{
    // ОБЩИЕ ДАННЫЕ И ЗАКАЗЧИК
    public string ProjectNumber { get; set; } = string.Empty;
    public string CabinetName { get; set; } = string.Empty;
    public string DocDesignation { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string KpNumber { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;

    // ВОЗДУШНЫЙ КЛАПАН ПРИТОЧНЫЙ
    public string ValveInVoltage { get; set; } = "230 В";
    public bool ValveInSpring { get; set; }
    public string ValveInControl { get; set; } = "Откр/Закр";
    public bool ValveInFeedback { get; set; }
    public bool ValveInHeating { get; set; }
    public double ValveInPower { get; set; }
    public double ValveInCurrent { get; set; }

    // ВОЗДУШНЫЙ КЛАПАН ВЫТЯЖНОЙ
    public string ValveOutVoltage { get; set; } = "230 В";
    public bool ValveOutSpring { get; set; }
    public string ValveOutControl { get; set; } = "Откр/Закр";
    public bool ValveOutFeedback { get; set; }
    public bool ValveOutHeating { get; set; }
    public double ValveOutPower { get; set; }
    public double ValveOutCurrent { get; set; }

    // НАГРЕВАТЕЛИ ВОДЯНЫЕ
    public double HeaterW1PumpPower { get; set; }
    public double HeaterW1PumpCurrent { get; set; }
    public string HeaterW1Voltage { get; set; } = "230 В/1ф";
    public bool HeaterW1ValveFb { get; set; }
    public string HeaterW1ValveCtrl { get; set; } = "Откр/Закр";

    public double HeaterW2PumpPower { get; set; }
    public double HeaterW2PumpCurrent { get; set; }
    public string HeaterW2Voltage { get; set; } = "230 В/1ф";
    public bool HeaterW2ValveFb { get; set; }
    public string HeaterW2ValveCtrl { get; set; } = "Откр/Закр";

    // НАГРЕВАТЕЛИ ЭЛЕКТРИЧЕСКИЕ
    public string Heater1Type { get; set; } = "Нет";
    public double HeaterEl1Power { get; set; }
    public string HeaterEl1Voltage { get; set; } = "400 В/3ф";
    public string HeaterEl1Control { get; set; } = "Плавное (ШИМ регулятор)";

    public double HeaterEl2Power { get; set; }
    public string HeaterEl2Voltage { get; set; } = "400 В/3ф";
    public string HeaterEl2Control { get; set; } = "Плавное (ШИМ регулятор)";

    // ОХЛАДИТЕЛИ
    public double CoolerWPumpPower { get; set; }
    public double CoolerWPumpCurrent { get; set; }
    public string CoolerWVoltage { get; set; } = "230 В/1ф";
    public bool CoolerWValveFb { get; set; }
    public string CoolerWValveCtrl { get; set; } = "Откр/Закр";

    public string CoolerFrStages { get; set; } = "Нет";
    public bool CoolerFrHpAlarm { get; set; }
    public bool CoolerFrLpAlarm { get; set; }
    public bool CoolerFrCrankcaseHeat { get; set; }

    // УТИЛИЗАЦИЯ (РЕКУПЕРАЦИЯ)
    public bool RecirculationExist { get; set; }
    public string RecirculationCtrl { get; set; } = "Дискретное";
    public bool RecPlateExist { get; set; }
    public bool RecPlateBypass { get; set; }
    public string RecPlateBypassCtrl { get; set; } = "Дискретное";
    public bool RecRotorExist { get; set; }
    public string RecRotorCtrl { get; set; } = "Прямой пуск";
    public string RecRotorVoltage { get; set; } = "230 В/1ф";
    public bool RecGlycolExist { get; set; }
    public double RecGlycolPumpPower { get; set; }
    public double RecGlycolPumpCurrent { get; set; }
    public string RecGlycolVoltage { get; set; } = "230 В/1ф";
    public string RecGlycolCtrl { get; set; } = "Откр/Закр";

    // УВЛАЖНЕНИЕ И ОСУШЕНИЕ
    public bool HumCellExist { get; set; }
    public double HumCellPower { get; set; }
    public double HumCellCurrent { get; set; }
    public string HumCellVoltage { get; set; } = "230 В/1ф";
    public bool HumSteamExist { get; set; }
    public string HumSteamCtrl { get; set; } = "Вкл/Выкл";
    public bool HumSprayExist { get; set; }
    public double HumSprayPower { get; set; }
    public double HumSprayCurrent { get; set; }
    public string HumSprayVoltage { get; set; } = "230 В/1ф";
    public string HumSprayCtrl { get; set; } = "Вкл/Выкл";
    public string HumSensorType { get; set; } = "Нет";
    public bool DehumidificationExist { get; set; }

    // ВЕНТИЛЯТОРЫ (ПРИТОК И ВЫТЯЖКА)
    public string FanInVoltage { get; set; } = "400 В/3ф";
    public double SupplyFanPowerKw { get; set; }
    public double FanInCurrent { get; set; }
    public bool FanInReserve { get; set; }
    public bool FanInTwin { get; set; }
    public string SupplyFanRegulation { get; set; } = "Прямой пуск";
    public string FanInSpeedSel { get; set; } = "С контроллера";
    public string FanInControlType { get; set; } = "Реле давления";

    public string FanOutVoltage { get; set; } = "400 В/3ф";
    public double FanOutPower { get; set; }
    public double FanOutCurrent { get; set; }
    public bool FanOutReserve { get; set; }
    public bool FanOutTwin { get; set; }
    public string FanOutRegulation { get; set; } = "Прямой пуск";
    public string FanOutSpeedSel { get; set; } = "С контроллера";
    public string FanOutControlType { get; set; } = "Реле давления";
    public string FanOutManagement { get; set; } = "Сблокированное с приточным вентилятором";

    // УПРАВЛЕНИЕ И ДОПОЛНИТЕЛЬНО
    public bool HmiWallMono { get; set; }
    public bool HmiDoorMono { get; set; }
    public bool Hmi7Touch { get; set; }
    public bool Hmi43Touch { get; set; }
    public bool RemoteControlExist { get; set; }
    public string EnclosureType { get; set; } = "Пластик IP41";
    public string BreakerBrand { get; set; } = "KEAZ";
    public string BreakerSeries { get; set; } = "Пром. Серия 6 kA";
    public bool LampFire { get; set; }
    public bool LampAlarm { get; set; }
    public bool LampFilter { get; set; }
    public bool LampRun { get; set; }
    public bool LampPower { get; set; }
    public int FilterControlCount { get; set; }
    public bool AddSensorOutside { get; set; }
    public bool AddSensorExtract { get; set; }
    public int Analog010vCount { get; set; }
    public int DigitalDryContactCount { get; set; }
    public bool FullSystemReserve { get; set; }
    public string NetworkProtocol { get; set; } = "RS-485 (Modbus RTU)";
}

public class SelectedComponent
{
    public string Designation { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public double Current { get; set; }
}
