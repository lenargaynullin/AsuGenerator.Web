using System;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

public class ShuvStrategy : ICabinetStrategy
{
    public string CabinetType => "Шкаф управления вентиляцией (ШУВ)";

    public List<SelectedComponent> CalculateComponents(UiConfigInput input)
    {
        // Симулируем работу параметрического ядра под вентиляцию
        var config = new VentAvtomatikaConfig
        {
            ClientName = "Прямой конфигуратор сайта",
            CompanyName = "B2B Заказчик",
            KpNumber = input.ProjectNumber,
            Heater1Type = $"{input.TechnologyType} нагреватель калорифера",
            HeaterEl1Voltage = input.VoltageType,
            HeaterEl1Power = input.TechnologyType == "Электрический" ? 4.5f : 0f
        };

        // Вызываем ваше готовое параметрическое ядро
        var engine = new CalculationEngine();
        return engine.RunB2bLogic(config);
    }

    public CommercialProposal CalculateProposal(List<SelectedComponent> components, UiConfigInput input, decimal margin, PriceCalculationService priceCalc)
    {
        var config = new VentAvtomatikaConfig
        {
            ClientName = "Прямой конфигуратор сайта",
            CompanyName = "B2B Заказчик",
            KpNumber = input.ProjectNumber
        };

        // Рассчитываем смету ТКП через ваш готовый сервис цен
        return priceCalc.CalculateProposal(components, config, margin);
    }

    public Dictionary<string, byte[]> GenerateCadDrawings(List<SelectedComponent> components, UiConfigInput input, CadGeneratorService cadGen)
    {
        var config = new VentAvtomatikaConfig
        {
            ClientName = "Прямой конфигуратор сайта",
            CompanyName = "B2B Заказчик",
            KpNumber = input.ProjectNumber,
            Heater1Type = $"{input.TechnologyType} нагреватель калорифера",
            HeaterEl1Voltage = input.VoltageType
        };

        // Генерируем DXF-чертежи через ваш CAD-движок
        return cadGen.GenerateProjectSchematics(components, config);
    }
}
