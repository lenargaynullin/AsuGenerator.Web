using AsuGenerator.Web.Core.Models;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

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
}
