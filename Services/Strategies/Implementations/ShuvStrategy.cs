using System;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

public class ShuvStrategy : ICabinetStrategy
{
    public string CabinetType => "Шкаф управления вентиляцией (ШУВ)";

        public List<SelectedComponent> CalculateComponents(UiConfigInput input)
    {
        var components = new List<SelectedComponent>();

        // ========== ВСЕГДА: общие компоненты ==========
        components.Add(new() { Designation = "QS1", Article = "17014DEK", Description = "Выключатель-разъединитель ВН-102 4P 32А", Vendor = "Dekraft" });
        components.Add(new() { Designation = "QF1", Article = "21231DEK", Description = "Автомат защиты двигателя 3P 13-18A", Vendor = "Dekraft" });
        components.Add(new() { Designation = "KM1", Article = "18053DEK", Description = "Контактор модульный 4НО 16А", Vendor = "Dekraft" });
        components.Add(new() { Designation = "A1", Article = input.BaseConfig?.PlcType ?? "ПР200-24.4.2.0", Description = "Программируемое реле", Vendor = "ОВЕН" });
        components.Add(new() { Designation = "G1", Article = "БП60Б-Д4-24", Description = "Блок питания 60Вт 24В", Vendor = "ОВЕН" });

        // ========== ЛАМПЫ, КНОПКИ (всегда) ==========
        components.Add(new() { Designation = "HL1", Article = "BLS10-ADDS-230-K05", Description = "Лампа Сеть", Vendor = "IEK" });
        components.Add(new() { Designation = "HL2", Article = "BLS10-ADDS-230-K06", Description = "Лампа Работа", Vendor = "IEK" });
        components.Add(new() { Designation = "HL3", Article = "BLS10-ADDS-230-K04", Description = "Лампа Авария", Vendor = "IEK" });
        components.Add(new() { Designation = "SB1", Article = "BBT61-BA-K04", Description = "Кнопка Стоп", Vendor = "IEK" });
        components.Add(new() { Designation = "SB2", Article = "BBT50-BW-K06", Description = "Кнопка Пуск", Vendor = "IEK" });
        components.Add(new() { Designation = "SA1", Article = "BSW60-BD-3-K02", Description = "Переключатель 3 поз.", Vendor = "IEK" });

        // ========== ВОДЯНОЙ НАГРЕВ ==========
        if (input.TechnologyType == "Водяной")
        {
            components.Add(new() { Designation = "TE1", Article = "ДТС3125-PT1000", Description = "Датчик температуры наружного воздуха", Vendor = "ОВЕН" });
            components.Add(new() { Designation = "TE2", Article = "ДТС3222-PT1000", Description = "Датчик температуры обратной воды", Vendor = "ОВЕН" });
            components.Add(new() { Designation = "TSA1", Article = "MTR-K3", Description = "Капиллярный термостат защиты от замерзания", Vendor = "Meyertec" });
            components.Add(new() { Designation = "KL5", Article = "MR-207A", Description = "Реле 220V AC для привода клапана", Vendor = "КИППРИБОР" });
            components.Add(new() { Designation = "M2", Article = "", Description = "Циркуляционный насос калорифера", Vendor = "По проекту" });
        }

        // ========== ЭЛЕКТРИЧЕСКИЙ НАГРЕВ ==========
        if (input.TechnologyType == "Электрический")
        {
            components.Add(new() { Designation = "TT1", Article = "ТТР-40А", Description = "Твердотельное реле 40А для ШИМ-регулирования", Vendor = "ОВЕН" });
            components.Add(new() { Designation = "KM4", Article = "18053DEK", Description = "Контактор модульный 4НО 16А для ТЭНов", Vendor = "Dekraft", Quantity = 3 });
            components.Add(new() { Designation = "QF7", Article = "21231DEK", Description = "Автомат защиты 3P 18A для ТЭНов", Vendor = "Dekraft", Quantity = 3 });
            components.Add(new() { Designation = "TE1", Article = "ДТС3125-PT1000", Description = "Датчик температуры приточного воздуха", Vendor = "ОВЕН" });
        }

        // ========== MODBUS ==========
        if (input.BaseConfig?.Protocol == "Modbus RTU" || input.BaseConfig?.Protocol == "Modbus TCP")
        {
            components.Add(new() { Designation = "A2", Article = "ПРМ-24.1", Description = "Модуль расширения ПРМ-24.1", Vendor = "ОВЕН" });
        }

        // ========== ОБОГРЕВ ШКАФА ==========
        if (input.BaseConfig?.HasHeater == true)
        {
            components.Add(new() { Designation = "EK1", Article = "ТЭН-50Вт", Description = "Обогреватель шкафа 50Вт", Vendor = "IEK" });
            components.Add(new() { Designation = "QF8", Article = "12267DEK", Description = "Автомат 1P 4A для обогрева", Vendor = "Dekraft" });
        }

        // ========== РЕЛЕ И КЛЕММЫ (всегда) ==========
        components.Add(new() { Designation = "KL1", Article = "MR-203D", Description = "Реле 24V DC", Vendor = "КИППРИБОР", Quantity = 3 });
        components.Add(new() { Designation = "XT1", Article = "YZN30-016-K03", Description = "Клемма 16 мм²", Vendor = "IEK", Quantity = 5 });
        components.Add(new() { Designation = "XT2", Article = "YZN30-002-K03", Description = "Клемма 2,5 мм²", Vendor = "IEK", Quantity = 19 });

        // ========== ОХЛАДИТЕЛЬ ==========
        if (input.TechnologyType == "Фреоновый")
        {
            components.Add(new() { Designation = "KM5", Article = "18050DEK", Description = "Контактор 2НО 16А для ККБ", Vendor = "Dekraft" });
            components.Add(new() { Designation = "QF9", Article = "21224DEK", Description = "Автомат защиты 3P 1A для ККБ", Vendor = "Dekraft" });
            components.Add(new() { Designation = "KA2", Article = "MR-203D", Description = "Реле сигнала высокого давления", Vendor = "КИППРИБОР" });
            components.Add(new() { Designation = "KA3", Article = "MR-203D", Description = "Реле сигнала низкого давления", Vendor = "КИППРИБОР" });
        }

        if (input.TechnologyType == "Водяной охладитель")
        {
            components.Add(new() { Designation = "M3", Article = "", Description = "Циркуляционный насос охладителя", Vendor = "По проекту" });
            components.Add(new() { Designation = "KL6", Article = "MR-207A", Description = "Реле 220V AC для привода клапана охладителя", Vendor = "КИППРИБОР" });
            components.Add(new() { Designation = "QF10", Article = "21224DEK", Description = "Автомат защиты 3P 1A для насоса охладителя", Vendor = "Dekraft" });
            components.Add(new() { Designation = "TE3", Article = "ДТС3125-PT1000", Description = "Датчик температуры холодной воды", Vendor = "ОВЕН" });
        }

        // ========== УВЛАЖНИТЕЛЬ ==========
        if (input.HasHumidifier)
        {
            components.Add(new() { Designation = "M4", Article = "", Description = "Насос увлажнителя", Vendor = "По проекту" });
            components.Add(new() { Designation = "KM6", Article = "18050DEK", Description = "Контактор 2НО 16А для увлажнителя", Vendor = "Dekraft" });
            components.Add(new() { Designation = "QF11", Article = "21224DEK", Description = "Автомат защиты 3P 1A для увлажнителя", Vendor = "Dekraft" });
            components.Add(new() { Designation = "ME1", Article = "ПВТ100", Description = "Датчик влажности приточного воздуха", Vendor = "ОВЕН" });
        }

        // ========== РЕЗЕРВНЫЙ ВЕНТИЛЯТОР (АВР) ==========
        if (input.HasReserveFan)
        {
            components.Add(new() { Designation = "KM7", Article = "18053DEK", Description = "Контактор резервного вентилятора", Vendor = "Dekraft" });
            components.Add(new() { Designation = "QF12", Article = "21231DEK", Description = "Автомат защиты резервного вентилятора", Vendor = "Dekraft" });
            components.Add(new() { Designation = "KA4", Article = "MR-203D", Description = "Реле контроля фаз АВР", Vendor = "КИППРИБОР" });
            components.Add(new() { Designation = "KA5", Article = "MR-203D", Description = "Реле блокировки одновременного пуска", Vendor = "КИППРИБОР" });
        }

        // ========== ДИСПЕТЧЕРИЗАЦИЯ ==========
        if (input.HasDispatching)
        {
            components.Add(new() { Designation = "A3", Article = "ПРМ-24.1", Description = "Модуль расширения для диспетчеризации", Vendor = "ОВЕН" });
            components.Add(new() { Designation = "KA6", Article = "MR-203D", Description = "Реле сигнала Работа", Vendor = "КИППРИБОР" });
            components.Add(new() { Designation = "KA7", Article = "MR-203D", Description = "Реле сигнала Авария", Vendor = "КИППРИБОР" });
            components.Add(new() { Designation = "KA8", Article = "MR-203D", Description = "Реле сигнала Пожар", Vendor = "КИППРИБОР" });
        }

        // ========== ДОПОЛНИТЕЛЬНЫЕ ДАТЧИКИ ==========
        if (input.HasAdditionalSensors)
        {
            components.Add(new() { Designation = "PDS1", Article = "РД30-ДД500", Description = "Датчик перепада давления фильтра 1", Vendor = "ОВЕН" });
            components.Add(new() { Designation = "PDS2", Article = "РД30-ДД500", Description = "Датчик перепада давления фильтра 2", Vendor = "ОВЕН" });
            components.Add(new() { Designation = "PDS3", Article = "РД30-ДД1000", Description = "Реле давления приточного вентилятора", Vendor = "ОВЕН" });
        }

        return components;
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
