using System;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

public class CalculationEngine
{
    // Главный метод ядра, который принимает распарсенные данные и возвращает полную b2b-спецификацию
    public List<SelectedComponent> RunB2bLogic(VentAvtomatikaConfig config)
    {
        var bomList = new List<SelectedComponent>();

        // 1. ПОДБОР СИЛЫ: Приточный вентилятор
        if (config.SupplyFanPowerKw > 0)
        {
            var fanEquipment = SelectFanEquipment(config.SupplyFanPowerKw, config.SupplyFanRegulation);
            bomList.AddRange(fanEquipment);
        }

        // 2. ПОДБОР СИЛЫ: Насос водяного калорифера (ГВС)
        if (config.Heater1Type == "Водяной")
        {
            var pumpEquipment = SelectWaterHeaterEquipment(config.HeaterW1PumpPower);
            bomList.AddRange(pumpEquipment);
        }

        // 3. ПОДБОР СИЛЫ: Электрический калорифер (ТЭНы + ТТР + Контактор защиты)
        if (config.Heater1Type == "Электрический")
        {
            var heaterEquipment = SelectElectricHeaterEquipment(config.HeaterEl1Power);
            bomList.AddRange(heaterEquipment);
        }

        // 4. ПОДБОР АВТОМАТИКИ: Базовый комплект (ПЛК + Блок питания + Защита управления)
        var plcEquipment = SelectBaseAutomation(config);
        bomList.AddRange(plcEquipment);

        return bomList;
    }

    private List<SelectedComponent> SelectFanEquipment(double power, string startType)
    {
        var list = new List<SelectedComponent>();

        // Инженерный расчет трехфазного тока (I = P * 2.15)
        double current = Math.Round(power * 2.15, 2);

        // Подбор автомата защиты двигателя КЭАЗ (серия OptiStart MP)
        var breaker = new SelectedComponent { Designation = "QF1", Vendor = "КЭАЗ", Current = current };

        if (current <= 0.16) { breaker.Article = "OptiStart MP-0,16"; breaker.Description = "Автомат защиты двигателя (уставка 0.1-0.16А)"; }
        else if (current <= 0.4) { breaker.Article = "OptiStart MP-0,4"; breaker.Description = "Автомат защиты двигателя (уставка 0.25-0.4А)"; }
        else if (current <= 1.0) { breaker.Article = "OptiStart MP-1,0"; breaker.Description = "Автомат защиты двигателя (уставка 0.63-1.0А)"; }
        else if (current <= 1.6) { breaker.Article = "OptiStart MP-1,6"; breaker.Description = "Автомат защиты двигателя (уставка 1.0-1.6А)"; }
        else if (current <= 2.5) { breaker.Article = "OptiStart MP-2,5"; breaker.Description = "Автомат защиты двигателя (уставка 1.6-2.5А)"; }
        else if (current <= 4.0) { breaker.Article = "OptiStart MP-4,0"; breaker.Description = "Автомат защиты двигателя (уставка 2.5-4.0А)"; }
        else if (current <= 6.3) { breaker.Article = "OptiStart MP-6,3"; breaker.Description = "Автомат защиты двигателя (уставка 4.0-6.3А)"; }
        else if (current <= 10.0) { breaker.Article = "OptiStart MP-10"; breaker.Description = "Автомат защиты двигателя (уставка 6.3-10А)"; }
        else if (current <= 16.0) { breaker.Article = "OptiStart MP-16"; breaker.Description = "Автомат защиты двигателя (уставка 10-16А)"; }
        else { breaker.Article = "OptiStart MP-32"; breaker.Description = "Автомат защиты двигателя (уставка 25-32А)"; }

        list.Add(breaker);

        // Выбор коммутации: Частотник ОВЕН или Контактор КЭАЗ
        if (startType == "Частотное")
        {
            var vfd = new SelectedComponent { Designation = "UZ1", Vendor = "ОВЕН" };

            if (power <= 0.75) { vfd.Article = "ПЧВ1-К75-Б"; vfd.Description = "Преобразователь частоты 0.75кВт 220В (1-фазный вход)"; }
            else if (power <= 1.5) { vfd.Article = "ПЧВ1-1К5-Б"; vfd.Description = "Преобразователь частоты 1.5кВт 220В (1-фазный вход)"; }
            else if (power <= 2.2) { vfd.Article = "ПЧВ1-2К2-В"; vfd.Description = "Преобразователь частоты 2.2кВт 380В (3-фазный вход)"; }
            else if (power <= 5.5) { vfd.Article = "ПЧВ2-5К5-В"; vfd.Description = "Преобразователь частоты 5.5кВт 380В (3-фазный вход)"; }
            else { vfd.Article = "ПЧВ2-11К-В"; vfd.Description = "Преобразователь частоты 11кВт 380В (3-фазный вход)"; }

            list.Add(vfd);
        }
        else
        {
            var contactor = new SelectedComponent { Designation = "KM1", Vendor = "КЭАЗ" };

            if (current <= 9) { contactor.Article = "OptiStart K-09.10-230AC"; contactor.Description = "Контактор магнитный 9А (управление 230В)"; }
            else if (current <= 12) { contactor.Article = "OptiStart K-12.10-230AC"; contactor.Description = "Контактор магнитный 12А (управление 230В)"; }
            else { contactor.Article = "OptiStart K-18.10-230AC"; contactor.Description = "Контактор магнитный 18А (управление 230В)"; }

            list.Add(contactor);
        }

        return list;
    }

    private List<SelectedComponent> SelectWaterHeaterEquipment(double pumpPower)
    {
        var list = new List<SelectedComponent>();

        if (pumpPower <= 0) return list;

        // Защищаем цепь мелкого циркуляционного насоса b2b-автоматом КЭАЗ ВА47-29
        var pumpBreaker = new SelectedComponent { Designation = "SF2", Vendor = "КЭАЗ" };

        if (pumpPower <= 0.25)
        {
            pumpBreaker.Article = "ВА47-29 1P 2A C";
            pumpBreaker.Description = $"Автомат защиты насоса калорифера ({pumpPower} кВт)";
        }
        else
        {
            pumpBreaker.Article = "ВА47-29 1P 4A C";
            pumpBreaker.Description = $"Автомат защиты насоса калорифера ({pumpPower} кВт)";
        }
        list.Add(pumpBreaker);

        // Добавляем миниконтактор для управления насосом ГВС от релейного выхода ПЛК
        list.Add(new SelectedComponent
        {
            Designation = "KM2",
            Vendor = "КЭАЗ",
            Article = "OptiStart K-09.10-230AC",
            Description = "Контактор включения циркуляционного насоса ГВС"
        });

        return list;
    }

    private List<SelectedComponent> SelectElectricHeaterEquipment(double powerKw)
    {
        var list = new List<SelectedComponent>();
        if (powerKw <= 0) return list;

        // Расчет тока трехфазного нагревателя: I = P / (1.732 * 0.38) = P * 1.52
        double current = Math.Round(powerKw * 1.52, 2);

        // 1. Защитный автомат КЭАЗ серии ВА47-100 под ТЭНы (высокая стойкость к перегрузкам)
        var heaterBreaker = new SelectedComponent { Designation = "QF2", Vendor = "КЭАЗ", Current = current };

        if (current <= 10) { heaterBreaker.Article = "ВА47-100 3P 16A C"; }
        else if (current <= 20) { heaterBreaker.Article = "ВА47-100 3P 25A C"; }
        else if (current <= 32) { heaterBreaker.Article = "ВА47-100 3P 40A C"; }
        else { heaterBreaker.Article = "ВА47-100 3P 63A C"; }

        heaterBreaker.Description = $"Автомат защиты электрокалорифера {powerKw} кВт";
        list.Add(heaterBreaker);

        // 2. Силовой контактор безопасности КЭАЗ (разрывает питание ТЭНов по сигналу аварии/перегрева)
        var safetyContactor = new SelectedComponent { Designation = "KM3", Vendor = "КЭАЗ" };

        if (current <= 9) { safetyContactor.Article = "OptiStart K-09.10-230AC"; }
        else if (current <= 12) { safetyContactor.Article = "OptiStart K-12.10-230AC"; }
        else if (current <= 18) { safetyContactor.Article = "OptiStart K-18.10-230AC"; }
        else { safetyContactor.Article = "OptiStart K-25.10-230AC"; }

        safetyContactor.Description = "Контактор аварийной защиты от перегрева калорифера";
        list.Add(safetyContactor);

        // 3. Твердотельное реле (ТТР) ОВЕН для плавного ШИМ-управления температурой воздуха
        list.Add(new SelectedComponent
        {
            Designation = "UZ2",
            Vendor = "ОВЕН",
            Article = "HD-4044.ZD3",
            Description = "Твердотельное реле трехфазное (коммутация ТЭНов через ШИМ)"
        });

        return list;
    }

    private List<SelectedComponent> SelectBaseAutomation(VentAvtomatikaConfig config)
    {
        var list = new List<SelectedComponent>();

        // Программируем базу: Любому шкафу ЩУВ нужен контроллер и питание
        list.Add(new SelectedComponent
        {
            Designation = "DD1",
            Vendor = "ОВЕН",
            Article = "ПР200-24.4.2.0",
            Description = "Программируемое реле с дисплеем, питание 24В"
        });

        list.Add(new SelectedComponent
        {
            Designation = "G1",
            Vendor = "ОВЕН",
            Article = "БП30Б-Д4-24",
            Description = "Блок питания на DIN-рейку 30Вт, 24В"
        });

        list.Add(new SelectedComponent
        {
            Designation = "SF1",
            Vendor = "КЭАЗ",
            Article = "ВА47-29 1P 6A C",
            Description = "Автоматический выключатель защиты цепей управления"
        });

        return list;
    }
}
