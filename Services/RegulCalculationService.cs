using System;
using System.Collections.Generic;
using System.Linq;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

public class RegulCalculationService
{
    private const int MaxSlotsPerRack = 40;        // Лимит модулей в крейте по БД
    private const double MaxBusCurrentA = 4.5;      // Лимит тока шины питания по БД
    private const double PowerModuleCapacityA = 3.125; // Мощность БП
    private const double ReserveMultiplier = 1.20;  // 20% инженерного резерва из вашей БД

    // Метод 1: Полный расчет системы из информационной мощности с барьерами
    public List<RegulRackResult> CalculateSystemWithDbAndBarriers(List<IoSignalRow> uploadedSignals)
    {
        var requestedModules = new List<RegulModuleInfo>();
        var totalAccessories = new List<AssociatedAccessory>();

        // Автоматически добавляем резервированный процессор (ширина 80мм по вашей БД)
        requestedModules.Add(new RegulModuleInfo
        {
            PartNumber = "R500 CU 00 051",
            Description = "Модуль центрального процессора I-го типа",
            WidthMm = 80.0,
            CurrentConsumptionA = 0.45,
            PowerType = RegulPowerType.TwoBus,
            IsIoOrCpu = true
        });

        foreach (var signal in uploadedSignals)
        {
            string typeUpper = signal.SignalType.ToUpper().Trim();
            int countWithReserve = (int)Math.Ceiling(signal.TotalWithReserve * ReserveMultiplier);
            if (countWithReserve <= 0) continue;

            if (typeUpper.Contains("AI"))
            {
                int modulesCount = (int)Math.Ceiling((double)countWithReserve / 8);
                for (int i = 0; i < modulesCount; i++)
                {
                    requestedModules.Add(new RegulModuleInfo { PartNumber = "R500 AI 08 041", Description = "Модуль аналогового ввода, 8 каналов", WidthMm = 40.0, CurrentConsumptionA = 0.18, PowerType = RegulPowerType.TwoBus, IsIoOrCpu = true });
                }

                int barriersCount = (int)Math.Ceiling((double)countWithReserve / 2);
                totalAccessories.Add(new AssociatedAccessory { PartNumber = "Барьер AI (2-канал)", Name = "Барьер искрозащиты AI 2-канальный", Quantity = barriersCount, Type = "Барьер", WidthMm = 12.5 });
            }
            else if (typeUpper.Contains("DI"))
            {
                int modulesCount = (int)Math.Ceiling((double)countWithReserve / 32);
                for (int i = 0; i < modulesCount; i++)
                {
                    requestedModules.Add(new RegulModuleInfo { PartNumber = "R500 DI 32 011", Description = "Модуль дискретного ввода, 32 канала", WidthMm = 40.0, CurrentConsumptionA = 0.12, PowerType = RegulPowerType.TwoBus, IsIoOrCpu = true });
                }

                int barriersCount = (int)Math.Ceiling((double)countWithReserve / 4);
                totalAccessories.Add(new AssociatedAccessory { PartNumber = "Барьер DI (4-канал)", Name = "Барьер искрозащиты DI 4-канальный", Quantity = barriersCount, Type = "Барьер", WidthMm = 12.5 });
            }
            else if (typeUpper.Contains("DO"))
            {
                int modulesCount = (int)Math.Ceiling((double)countWithReserve / 32);
                for (int i = 0; i < modulesCount; i++)
                {
                    requestedModules.Add(new RegulModuleInfo { PartNumber = "R500 DO 32 011", Description = "Модуль дискретного вывода, 32 канала", WidthMm = 40.0, CurrentConsumptionA = 0.15, PowerType = RegulPowerType.TwoBus, IsIoOrCpu = true });
                }

                totalAccessories.Add(new AssociatedAccessory { PartNumber = "Барьер DO (1-канал)", Name = "Барьер искрозащиты DO 1-канальный", Quantity = countWithReserve, Type = "Барьер", WidthMm = 12.5 });
            }
        }

        // Вызываем внутренний метод распределения
        var racks = BuildRegulArchitecture(requestedModules);

        if (racks.Any())
        {
            racks.First().Accessories = totalAccessories;
            racks.First().TotalAccessoriesWidthMm = totalAccessories.Sum(a => a.WidthMm * a.Quantity);
        }

        return racks;
    }

    // Метод 2: Распределение крейтов горизонтально по шкафам Провенто
    public List<RegulCabinetResult> DistributeRacksToCabinets(List<RegulRackResult> calculatedRacks, double selectedWidthMm)
    {
        var cabinets = new List<RegulCabinetResult>();
        int cabinetCounter = 1;

        var currentCabinet = new RegulCabinetResult { CabinetIndex = cabinetCounter, EnclosureWidthMm = selectedWidthMm };

        foreach (var rack in calculatedRacks)
        {
            if (rack.TotalRackWidthMm > currentCabinet.UsefulWidthMm)
            {
                throw new InvalidOperationException($"Крейт №{rack.RackIndex} имеет ширину {rack.TotalRackWidthMm} мм и не помещается в шкаф {selectedWidthMm} мм. Выберите шкаф большего габарита.");
            }

            if (currentCabinet.RacksInCabinet.Any())
            {
                cabinets.Add(currentCabinet);
                cabinetCounter++;
                currentCabinet = new RegulCabinetResult { CabinetIndex = cabinetCounter, EnclosureWidthMm = selectedWidthMm };
            }

            currentCabinet.RacksInCabinet.Add(rack);
        }

        if (currentCabinet.RacksInCabinet.Any()) cabinets.Add(currentCabinet);
        return cabinets;
    }

    // ИСПРАВЛЕНО: Метод теперь находится внутри этого же класса и доступен в текущем контексте
    public List<RegulRackResult> BuildRegulArchitecture(List<RegulModuleInfo> requestedModules)
    {
        var resultSpecs = new List<RegulRackResult>();
        int rackCounter = 1;

        // Сортируем модули: сначала процессоры (CU), затем блоки питания (PP), затем ввод/вывод
        var moduleQueue = new Queue<RegulModuleInfo>(
            requestedModules.OrderByDescending(m => m.PartNumber.Contains("CU"))
        );

        // Лимит полезной длины DIN-рейки для шкафа 800мм (800 - 100мм на кабель-каналы)
        const double MaxRowWidthMm = 700.0;

        while (moduleQueue.Count > 0)
        {
            var currentRack = new RegulRackResult { RackIndex = rackCounter };
            var currentRackModules = new List<RegulModuleInfo>();

            // Базовая физическая ширина элементов управления крейта
            // Левый оконечник шины IN (40 мм) + два БП (по 40 мм) = 120 мм начальной ширины
            double currentWidthMm = 120.0;

            bool isFirstRack = (rackCounter == 1);
            bool needsNextRow = false;

            // Набиваем модули в текущий горизонтальный ряд, пока не упремся в 700 мм
            while (moduleQueue.Count > 0)
            {
                var nextModule = moduleQueue.Peek();

                // Если это не первый крейт, в начале ряда железно встанет модуль расширения IN (40мм)
                double moduleWidth = nextModule.WidthMm;

                // Проверяем, поместится ли модуль + запас под правый оконечник OUT (40мм)
                if (currentWidthMm + moduleWidth + 40.0 > MaxRowWidthMm)
                {
                    needsNextRow = true;
                    break; // Текущий ряд заполнен, уходим на деление по вертикали
                }

                // Модуль помещается — забираем из очереди
                var module = moduleQueue.Dequeue();
                currentRackModules.Add(module);
                currentWidthMm += moduleWidth;
            }

            // Вычисляем суммарный ток шины текущего ряда
            double totalCurrent = currentRackModules.Sum(m => m.CurrentConsumptionA);
            currentRack.CalculatedBusCurrentA = totalCurrent;

            if (totalCurrent > MaxBusCurrentA)
            {
                throw new InvalidOperationException($"Превышен лимит тока шины 4.5А ({totalCurrent}А) на рейке №{rackCounter}.");
            }

            // --- ФОРМИРОВАНИЕ СПЕЦИФИКАЦИИ ТЕКУЩЕГО РЯДА (ГОСТ 21.110) ---

            // 1. Левый оконечник IN (R500 ST 01 012)
            currentRack.AddedComponents.Add(new CustomTemplateItem { PartNumber = "R500 ST 01 012", Name = "Модуль оконечный с функцией расширения шины (IN)", Quantity = 1, Unit = "шт", Type = "ПЛК" });

            // 2. Блоки питания (Монтируем по 2 БП на крейт для резервирования шины TwoBus)
            currentRack.AddedComponents.Add(new CustomTemplateItem { PartNumber = "R500 PP 00 021", Name = "Модуль источника питания 24 В DC, 75 Вт", Quantity = 2, Unit = "шт", Type = "ПЛК" });
            currentRack.AddedComponents.Add(new CustomTemplateItem { PartNumber = "R500 CH 02 022", Name = "Модуль шасси БП (80 мм)", Quantity = 2, Unit = "шт", Type = "ПЛК" });

            // 3. Модули ввода/вывода и процессоры текущего ряда
            var groupedIo = currentRackModules.GroupBy(m => new { m.PartNumber, m.Description, m.WidthMm });
            foreach (var group in groupedIo)
            {
                currentRack.AddedComponents.Add(new CustomTemplateItem { PartNumber = group.Key.PartNumber, Name = group.Key.Description, Quantity = group.Count(), Unit = "шт", Type = "ПЛК" });

                // Автоподбор шасси из вашей новой b2b-базы данных
                string chassisArt = group.Key.WidthMm == 80 ? "R500 CH 02 022" : "R500 CH 01 011";
                string chassisName = group.Key.WidthMm == 80 ? "Модуль шасси с поддержкой резервирования" : "Модуль шасси стандартный";

                currentRack.AddedComponents.Add(new CustomTemplateItem { PartNumber = chassisArt, Name = chassisName, Quantity = group.Count(), Unit = "шт", Type = "ПЛК" });
            }

            // 4. Правый оконечник OUT (R500 ST 01 022) — ставится, если цепь расширения шины идет на следующий ряд
            currentRack.AddedComponents.Add(new CustomTemplateItem { PartNumber = "R500 ST 01 022", Name = "Модуль оконечный с функцией расширения шины (OUT)", Quantity = 1, Unit = "шт", Type = "ПЛК" });

            // Фиксируем честные физические габариты ряда с учетом оконечников
            currentRack.TotalRackWidthMm = currentWidthMm + 40.0; // Добавляем 40мм правого оконечника

            resultSpecs.Add(currentRack);
            rackCounter++;
        }

        return resultSpecs;
    }

}
