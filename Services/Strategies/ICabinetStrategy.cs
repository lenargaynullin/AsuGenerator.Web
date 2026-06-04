using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

public interface ICabinetStrategy
{
    // Уникальный маркер типа шкафа (например, "ШУВ", "ШУН")
    string CabinetType { get; }

    // Шаг 1: Подбор аппаратов по параметрам с экрана
    List<SelectedComponent> CalculateComponents(UiConfigInput input);

    // Шаг 2: Расчет сметы ТКП с учетом маржинальности
    CommercialProposal CalculateProposal(List<SelectedComponent> components, UiConfigInput input, decimal margin, PriceCalculationService priceCalc);

    // Шаг 3: Генерация специфических DXF-чертежей схемы Э3
    Dictionary<string, byte[]> GenerateCadDrawings(List<SelectedComponent> components, UiConfigInput input, CadGeneratorService cadGen);
}

// Универсальный B2B-контейнер параметров с экрана
public class UiConfigInput
{
    public string EnclosureType { get; set; } = string.Empty;
    public string VoltageType { get; set; } = string.Empty;
    public string TechnologyType { get; set; } = string.Empty; // Тип нагрева калорифера
    public string ProjectNumber { get; set; } = string.Empty;
}
