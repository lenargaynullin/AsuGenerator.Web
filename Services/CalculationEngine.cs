using System;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

public class CalculationEngine
{
    // Главный метод ядра, который принимает данные опросника ШУЭ и возвращает спецификацию
    public List<SelectedComponent> RunB2bLogic(VentAvtomatikaConfig config)
    {
        var bomList = new List<SelectedComponent>();

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

        // 3. БАЗОВАЯ АВТОМАТИКА ШКАФА ШУЭ (Контроллер ОВЕН ПР200 + Блок питания)
        bomList.Add(new SelectedComponent { Designation = "DD1", Vendor = "ОВЕН", Article = "ПР200-24.4.2.0", Description = "Программируемый контроллер управления обогревом" });
        bomList.Add(new SelectedComponent { Designation = "G1", Vendor = "ОВЕН", Article = "БП30Б-Д4-24", Description = "Блок питания контроллера 24В" });
        bomList.Add(new SelectedComponent { Designation = "SF1", Vendor = "КЭАЗ", Article = "ВА47-29 1P 6A C", Description = "Автомат защиты цепей автоматики" });

        return bomList;
    }
}
