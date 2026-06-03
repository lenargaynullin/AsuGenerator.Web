using System;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

public class CalculationEngine
{
    // Главный метод ядра, который принимает данные опросника ШУЭ и возвращает спецификацию
    public List<SelectedComponent> RunB2bLogic(VentAvtomatikaConfig config)
    {
        var bomList = new List<SelectedComponent>();

        // 3. БАЗОВАЯ СПЕЦИФИКАЦИЯ ШКАФА ЩУВ
        bomList.Add(new SelectedComponent { Designation = "A1", Vendor = "ОВЕН", Article = "ПР200-24.4.2.0", Description = "Программируемое реле с дисплеем" });
        bomList.Add(new SelectedComponent { Designation = "A2", Vendor = "ОВЕН", Article = "ПРМ-24.1", Description = "Модуль расширения для программируемого реле" });
        bomList.Add(new SelectedComponent { Designation = "G1", Vendor = "ОВЕН", Article = "БП60Б-Д4-24", Description = "Блок питания 60 Вт 24 В" });
        bomList.Add(new SelectedComponent { Designation = "HL1", Vendor = "IEK", Article = "BLS10-ADDS-230-K05", Description = "Лампа d22мм желтый 230В AC/DC" });
        bomList.Add(new SelectedComponent { Designation = "HL2", Vendor = "IEK", Article = "BLS10-ADDS-230-K06", Description = "Лампа d22мм зеленый 230В AC/DC" });
        bomList.Add(new SelectedComponent { Designation = "HL3", Vendor = "IEK", Article = "BLS10-ADDS-230-K04", Description = "Лампа d22мм красный 230В AC/DC" });
        bomList.Add(new SelectedComponent { Designation = "KL1...KL3", Vendor = "КИППРИБОР", Article = "MR-203D", Description = "Промежуточные реле в компактном корпусе 24 VDC, 2CO" });
        bomList.Add(new SelectedComponent { Designation = "KL1...KL6", Vendor = "КИППРИБОР", Article = "PYF-022BE/2", Description = "Колодка монтажная серий PYF-022BE (для 2-контактных промежуточных реле)" });
        bomList.Add(new SelectedComponent { Designation = "KL1...KL3", Vendor = "КИППРИБОР", Article = "BS 2/15P", Description = "Зажим пластмассовый удерживающий" });
        bomList.Add(new SelectedComponent { Designation = "KL1...KL3", Vendor = "КИППРИБОР", Article = "LM-СF 24 V AC/DC", Description = "Модуль LED-индикации 24 V AC/DC" });
        bomList.Add(new SelectedComponent { Designation = "KL4...KL6", Vendor = "КИППРИБОР", Article = "MR-207A", Description = "Промежуточные реле в компактном корпусе 220 VAC, 2CO" });
        bomList.Add(new SelectedComponent { Designation = "KL4...KL6", Vendor = "КИППРИБОР", Article = "BS 2/15P", Description = "Зажим пластмассовый удерживающий" });
        bomList.Add(new SelectedComponent { Designation = "KL4...KL6", Vendor = "КИППРИБОР", Article = "LM-EN 230 V AC/DC", Description = "Модуль LED-индикации 230 V AC/DC" });
        bomList.Add(new SelectedComponent { Designation = "KM1", Vendor = "Dekraft", Article = "18053DEK", Description = "Контактор модульный 4НО 16 А 230 В МК-103" });
        bomList.Add(new SelectedComponent { Designation = "KM2 KM3", Vendor = "Dekraft", Article = "18050DEK", Description = "Контактор модульный 2НО 16 А 230 В МК-103" });
        bomList.Add(new SelectedComponent { Designation = "QF1", Vendor = "Dekraft", Article = "21231DEK", Description = "Автоматический выключатель защиты двигателя 3P 13,0-18,0 A 15 кА ВА-431" });
        bomList.Add(new SelectedComponent { Designation = "QF2 QF3", Vendor = "Dekraft", Article = "21224DEK", Description = "Автоматический выключатель защиты двигателя 3P 0,63-1,0 A 100 кА ВА-431" });
        bomList.Add(new SelectedComponent { Designation = "QF2 QF3", Vendor = "Dekraft", Article = "21269DEK", Description = "Контакт дополнительный фронтальный 1НО+1НЗ для ВА-431" });
        bomList.Add(new SelectedComponent { Designation = "QF4", Vendor = "Dekraft", Article = "12267DEK", Description = "Автоматический выключатель ВА-103 1P 4 А C 6 кА" });
        bomList.Add(new SelectedComponent { Designation = "QF5 QF6", Vendor = "Dekraft", Article = "12280DEK", Description = "Автоматический выключатель ВА-103 2P 1 А C 6 кА" });
        bomList.Add(new SelectedComponent { Designation = "QS1", Vendor = "Dekraft", Article = "17014DEK", Description = "Выключатель-разъединитель ВН-102 4P 32 А 400 В" });
        bomList.Add(new SelectedComponent { Designation = "SA1", Vendor = "IEK", Article = "BSW60-BD-3-K02", Description = "Переключатель LAY5-BD33 3 позиции I-0-II стандартная ручка, с фиксацией" });
        bomList.Add(new SelectedComponent { Designation = "SA1", Vendor = "IEK", Article = "BDK11", Description = "Контактный блок для светосигнальной арматуры 1НЗ" });
        bomList.Add(new SelectedComponent { Designation = "SB3", Vendor = "IEK", Article = "BBT61-BA-K04", Description = "Кнопка управления LAY5-BA42 без подсветки красная, 1НЗ" });
        bomList.Add(new SelectedComponent { Designation = "SB4", Vendor = "IEK", Article = "BBT50-BW-K06", Description = "Кнопка с подсветкой зеленая 230 В 1НО" });
        bomList.Add(new SelectedComponent { Designation = "XT1", Vendor = "IEK", Article = "YZN30-016-K03", Description = "Клемма винтовая КВИ-16 мм2 серая" });
        bomList.Add(new SelectedComponent { Designation = "XT2", Vendor = "IEK", Article = "YZN30-002-K03", Description = "Клемма винтовая КВИ-2,5 мм² серая" });
        bomList.Add(new SelectedComponent { Designation = "XT3", Vendor = "IEK", Article = "YZN30-004D-K03", Description = "Клемма винтовая КВИ-4-2L двухуровневая 4 мм² серая" });
        bomList.Add(new SelectedComponent { Designation = "XT1", Vendor = "IEK", Article = "YZN30D-ZGL-016-K03", Description = "Заглушка для КВИ-16 мм² серая" });
        bomList.Add(new SelectedComponent { Designation = "XT2", Vendor = "IEK", Article = "YZN30D-ZGL-002-K03", Description = "Заглушка для КВИ-2,5 мм² серая" });
        bomList.Add(new SelectedComponent { Designation = "XT3", Vendor = "IEK", Article = "YZN30D-ZGL-004D-K03", Description = "Заглушка для КВИ-4-2L двухуровневой 4 мм² серая" });

        // ПОДБОР ОБОРУДОВАНИЯ ПО ОЛ


        /*
        // 1. ПОДБОР ВВОДНОГО АППАРАТА ЗАЩИТЫ (Номинал 32А, ПКС 10кА под ТЗ Газпрома)
        var mainBreaker = new SelectedComponent
        {
            Designation = "QF1",
            Vendor = config.BreakerBrand ?? "КЭАЗ"
        };

        if (mainBreaker.Vendor == "КЭАЗ" || mainBreaker.Vendor == "KEAZ")
        {
            mainBreaker.Article = "OptiDin ВМ63-Г-3П-32А-С"; // Промышленная серия 10кА
            mainBreaker.Description = "Выключатель автоматический вводной 3P 32А (10кА)";
        }
        else
        {
            mainBreaker.Article = "AV-6 3P 32A C"; // Универсальный EKF AVERES 10кА
            mainBreaker.Description = "Выключатель автоматический вводной 3P 32А (10кА)";
        }
        bomList.Add(mainBreaker);

        // 2. ЦИКЛИЧЕСКИЙ ПОДБОР ФИДЕРОВ ОБОГРЕВА (Берем данные из нового опросника)
        int linesCount = config.OutletsHeatingCount > 0 ? config.OutletsHeatingCount : 5; // Количество линий из ТЗ
        double linePower = config.HeaterEl1Power > 0 ? config.HeaterEl1Power : 1.5;      // Мощность одной секции в кВт

        // Расчет рабочего тока одной секции (однофазная нагрузка 220В: I = P / 0.22)
        double lineCurrent = Math.Round(linePower / 0.22, 2);

        for (int i = 1; i <= linesCount; i++)
        {
            // Подбираем дифференциальный автомат (АВДТ) для защиты каждой линии греющего кабеля
            var diffBreaker = new SelectedComponent
            {
                Designation = $"QFD{i}",
                Vendor = config.BreakerBrand ?? "КЭАЗ"
            };

            if (diffBreaker.Vendor == "КЭАЗ" || diffBreaker.Vendor == "KEAZ")
            {
                // Выбираем номинал АВДТ КЭАЗ (16А или 25А с ПКС 10кА в зависимости от тока)
                diffBreaker.Article = lineCurrent <= 13 ? "OptiDin ДВ63-16A-C30-10кА" : "OptiDin ДВ63-25A-C30-10кА";
                diffBreaker.Description = $"Автомат дифференциального тока фидера обогрева №{i} (30мА, 10кА)";
            }
            else
            {
                diffBreaker.Article = lineCurrent <= 13 ? "АВДТ-63 2P 16A C30 AVERES" : "АВДТ-63 2P 25A C30 AVERES";
                diffBreaker.Description = $"Автомат дифференциального тока фидера обогрева №{i} (30мА, 10кА)";
            }
            bomList.Add(diffBreaker);

            // Автоматически добавляем силовые выходные клеммы для подключения кабелей
            bomList.Add(new SelectedComponent
            {
                Designation = $"X1.{i}",
                Vendor = "КЭАЗ",
                Article = "ЗНИ-4 серый",
                Description = $"Клемма винтовая подключения греющего кабеля линии №{i}"
            });
        }
        */
        return bomList;
    }
}
