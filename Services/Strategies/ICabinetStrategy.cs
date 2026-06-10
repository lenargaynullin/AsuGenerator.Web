using AsuGenerator.Web.Models;
using AsuGenerator.Web.Services.Strategies;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services.Strategies;

public interface ICabinetStrategy
{
    // Уникальный маркер типа шкафа (например, "ШУВ", "ШУН")
    string CabinetType { get; }

    // Шаг 1: Подбор аппаратов по параметрам с экрана
    List<SelectedComponent> CalculateComponents(UiConfigInput input);

}

// Универсальный B2B-контейнер параметров с экрана
public class UiConfigInput
{
    public string EnclosureType { get; set; } = string.Empty;
    public string VoltageType { get; set; } = string.Empty;
    public string TechnologyType { get; set; } = string.Empty; // Тип нагрева калорифера
    public string ProjectNumber { get; set; } = string.Empty;
    public BaseCabinetConfig? BaseConfig { get; set; }  // ← ДОБАВИТЬ ЭТУ СТРОКУ
    public bool HasHumidifier { get; set; } = false;
    public bool HasReserveFan { get; set; } = false;
    public bool HasDispatching { get; set; } = false;
    public bool HasAdditionalSensors { get; set; } = false;
    public bool SupplyFan { get; set; } = true;
    public bool HasVfd { get; set; }
    public bool HasHeaterPump { get; set; }
    public bool HasDamper { get; set; }
    public List<HeatingLine>? HeatingLines { get; set; }
    /// <summary>
    /// Выбранный бренд автоматов защиты из выпадающего списка UI (КЭАЗ, Dekraft и т.д.)
    /// </summary>
    public string PreferredBrand { get; set; } = "Dekraft"; // Дефолтное значение для MVP

    /// <summary>
    /// Наличие насоса увлажнителя
    /// </summary>
    public bool HasHumidifierPump { get; set; }

    /// <summary>
    /// Наличие аварии или сигналов блока кондиционера / увлажнителя
    /// </summary>
    public bool HasCoolerBlock { get; set; }
    public bool HasAirConditioner { get; set; }
    public bool Has3WayValve { get; set; }
}
