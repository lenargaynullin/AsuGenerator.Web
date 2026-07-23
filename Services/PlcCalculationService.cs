using System;
using System.Collections.Generic;
using System.Linq;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services
{
    public class PlcCalculationService
    {
        public PlcCabinetCalculationResult CalculateMultiPlcCabinet(
            SignalRequirement totalSignals,
            PlcComponent selectedCpu,
            List<PlcComponent> availableModules,
            PlcComponent? busCoupler,
            int maxSlotsPerBasket = 8,
            int cabinetWidthMm = 800,
            int cabinetHeightMm = 2000,
            CabinetType cabinetType = CabinetType.SingleSided)
        {
            var result = new PlcCabinetCalculationResult();
            // test
            // 1. Динамический расчет полезной геометрии монтажной панели
            int usefulWidthMm = cabinetWidthMm - 40 - 60 - 120;
            if (usefulWidthMm < 380) usefulWidthMm = 380;
            int panelHeightMm = cabinetHeightMm - 70;

            // 2. Расчет общего числа необходимых каналов с учетом резерва по ГОСТ
            double coeff = 1 + (totalSignals.ReservePercent / 100);
            int totalReqDi = (int)Math.Ceiling(totalSignals.TotalDI * coeff);
            int totalReqDo = (int)Math.Ceiling(totalSignals.TotalDO * coeff);
            int totalReqAiStd = (int)Math.Ceiling((totalSignals.AiNisCurrent2W + totalSignals.AiNisCurrent4W) * coeff);
            int totalReqAiTemp = (int)Math.Ceiling(totalSignals.AiRtdNis3W * coeff);
            int totalReqAo = (int)Math.Ceiling(totalSignals.TotalAO * coeff);

            // 3. Вычитаем встроенные ресурсы первого процессора
            totalReqDi -= selectedCpu.DiChannels;
            totalReqDo -= selectedCpu.DoChannels;
            totalReqAiStd -= selectedCpu.AiChannels;
            totalReqAo -= selectedCpu.AoChannels;

            var allRequiredModules = new List<PlcComponent>();
            var (diMods, _) = PickModules(totalReqDi, availableModules.Where(m => m.Type == "DI"));
            allRequiredModules.AddRange(diMods);
            var (doMods, _) = PickModules(totalReqDo, availableModules.Where(m => m.Type == "DO"));
            allRequiredModules.AddRange(doMods);
            var (aiStdMods, _) = PickModules(totalReqAiStd, availableModules.Where(m => m.Type == "AI_Current"));
            allRequiredModules.AddRange(aiStdMods);
            var (aiTempMods, _) = PickModules(totalReqAiTemp, availableModules.Where(m => m.Type == "AI_Temp"));
            allRequiredModules.AddRange(aiTempMods);
            var (aoMods, _) = PickModules(totalReqAo, availableModules.Where(m => m.Type == "AO"));
            allRequiredModules.AddRange(aoMods);

            var remainingModules = new Queue<PlcComponent>(allRequiredModules);

            // Агрегируем реле и клеммы для распределения
            int relayCount = totalSignals.DiNisMcc230VAC + totalSignals.DiNisMcc220VDC + totalSignals.DoNisMcc230VAC + totalSignals.DoNisMcc220VDC;
            var frontSideMock = new CabinetSide();
            var mockBasket = new PlcBasket();
            mockBasket.Modules.AddRange(allRequiredModules);
            var mockNode = new PlcNode(); mockNode.Baskets.Add(mockBasket);
            frontSideMock.Nodes.Add(mockNode);
            var totalTerminals = CalculateCabinetTerminals(frontSideMock);

            int cabinetCounter = 1;
            int baseCabinetHeaderHeightMm = 310; // Короб 40 + БП 190 + два короба по 40 = 310 мм

            if (cabinetType == CabinetType.DoubleSided)
            {
                // --- РАСЧЕТ ДВУХСТОРОННЕГО ШКАФА ---
                while (remainingModules.Any() || cabinetCounter == 1)
                {
                    var cabinet = new PlcCabinet { CabinetIndex = cabinetCounter, Type = CabinetType.DoubleSided };
                    cabinet.FrontSide.TotalAllocatedHeightMm = baseCabinetHeaderHeightMm;

                    var activeNode = new PlcNode { NodeIndex = cabinetCounter };
                    int basketIdx = 0;

                    while (remainingModules.Any() || (cabinetCounter == 1 && basketIdx == 0))
                    {
                        int basketRequiredHeightMm = 265;
                        if (cabinet.FrontSide.TotalAllocatedHeightMm + basketRequiredHeightMm + 200 > panelHeightMm && basketIdx > 0) break;

                        var currentBasket = new PlcBasket { BasketIndex = basketIdx };
                        if (basketIdx == 0) currentBasket.Modules.Add(selectedCpu);

                        while (remainingModules.Any())
                        {
                            var nextModule = remainingModules.Peek();
                            if (currentBasket.Modules.Sum(m => m.WidthMm) + nextModule.WidthMm <= usefulWidthMm)
                                currentBasket.Modules.Add(remainingModules.Dequeue());
                            else break;
                        }

                        activeNode.Baskets.Add(currentBasket);
                        cabinet.FrontSide.TotalAllocatedHeightMm += basketRequiredHeightMm;
                        cabinet.FrontSide.TotalDinRailsCount++;
                        basketIdx++;
                        if (!remainingModules.Any()) break;
                    }
                    if (activeNode.Baskets.Any()) cabinet.FrontSide.Nodes.Add(activeNode);

                    cabinet.InterposingRelays = new RelayUnit { Count = relayCount };
                    cabinet.Terminals = totalTerminals;
                    cabinet.BackSide.TotalAllocatedHeightMm = 40;
                    cabinet.BackSide = PackBackSideEquipment(relayCount, totalTerminals, usefulWidthMm);
                    cabinet.RecommendedPowerSupply = CalculatePSU(cabinet.FrontSide.Nodes.Sum(n => n.Baskets.Sum(b => b.Modules.Count(m => m.Type != "CPU"))));

                    result.Cabinets.Add(cabinet);
                    cabinetCounter++;
                }
            }
            else
            {
                // --- РАСЧЕТ ОДНОСТОРОННЕГО ШКАФА (ПЛК сверху, Реле/Клеммы снизу) ---
                var backBasketsQueue = GetBackSideBasketsQueue(relayCount, totalTerminals, usefulWidthMm);

                while (remainingModules.Any() || backBasketsQueue.Any() || cabinetCounter == 1)
                {
                    var cabinet = new PlcCabinet { CabinetIndex = cabinetCounter, Type = CabinetType.SingleSided };
                    cabinet.SingleSide.TotalAllocatedHeightMm = baseCabinetHeaderHeightMm;
                    cabinet.InterposingRelays = new RelayUnit { Count = relayCount };
                    cabinet.Terminals = totalTerminals;

                    var activeNode = new PlcNode { NodeIndex = cabinetCounter };
                    int basketIdx = 0;

                    // 1. Сначала выкладываем ПЛК и модули расширения (Верхняя половина панели)
                    while (remainingModules.Any() || (cabinetCounter == 1 && basketIdx == 0))
                    {
                        int basketRequiredHeightMm = 265;
                        if (cabinet.SingleSide.TotalAllocatedHeightMm + basketRequiredHeightMm + 200 > panelHeightMm && basketIdx > 0) break;

                        var currentBasket = new PlcBasket { BasketIndex = basketIdx };
                        if (basketIdx == 0) currentBasket.Modules.Add(selectedCpu);

                        while (remainingModules.Any())
                        {
                            var nextModule = remainingModules.Peek();
                            if (currentBasket.Modules.Sum(m => m.WidthMm) + nextModule.WidthMm <= usefulWidthMm)
                                currentBasket.Modules.Add(remainingModules.Dequeue());
                            else break;
                        }

                        activeNode.Baskets.Add(currentBasket);
                        cabinet.SingleSide.TotalAllocatedHeightMm += basketRequiredHeightMm;
                        cabinet.SingleSide.TotalDinRailsCount++;
                        basketIdx++;
                        if (!remainingModules.Any()) break;
                    }

                    // 2. Двигаемся ниже по этой же панели: выкладываем реле и клеммы
                    while (backBasketsQueue.Any())
                    {
                        int basketRequiredHeightMm = 265;
                        if (cabinet.SingleSide.TotalAllocatedHeightMm + basketRequiredHeightMm + 200 > panelHeightMm) break;

                        var backBasket = backBasketsQueue.Dequeue();
                        backBasket.BasketIndex = basketIdx;

                        activeNode.Baskets.Add(backBasket);
                        cabinet.SingleSide.TotalAllocatedHeightMm += basketRequiredHeightMm;
                        cabinet.SingleSide.TotalDinRailsCount++;
                        basketIdx++;
                    }

                    if (activeNode.Baskets.Any()) cabinet.SingleSide.Nodes.Add(activeNode);
                    cabinet.RecommendedPowerSupply = CalculatePSU(cabinet.SingleSide.Nodes.Sum(n => n.Baskets.Sum(b => b.Modules.Count(m => m.Type == "DI" || m.Type == "DO" || m.Type.StartsWith("AI") || m.Type == "AO"))));

                    result.Cabinets.Add(cabinet);
                    cabinetCounter++;
                    if (cabinetCounter > 10) break;
                }
            }

            return result;
        }

        private Queue<PlcBasket> GetBackSideBasketsQueue(int relayCount, TerminalBlockRow terminals, int usefulWidthMm)
        {
            var queue = new Queue<PlcBasket>();
            int remainingRelayWidth = relayCount * 16;
            int remainingTerminalWidth = (terminals.GreyTerminalsCount + terminals.BlueTerminalsCount + terminals.PeTerminalsCount) * 5;

            while (remainingRelayWidth > 0)
            {
                int width = Math.Min(remainingRelayWidth, usefulWidthMm);
                var b = new PlcBasket(); b.Modules.Add(new PlcComponent { Article = "FINDER", Name = "Блок реле", Type = "RELAY_BLOCK", WidthMm = width });
                queue.Enqueue(b);
                remainingRelayWidth -= width;
            }
            while (remainingTerminalWidth > 0)
            {
                int width = Math.Min(remainingTerminalWidth, usefulWidthMm);
                var b = new PlcBasket(); b.Modules.Add(new PlcComponent { Article = "IEK_COL", Name = "Клеммный ряд", Type = "TERMINAL_BLOCK", WidthMm = width });
                queue.Enqueue(b);
                remainingTerminalWidth -= width;
            }
            return queue;
        }

        private CabinetSide PackBackSideEquipment(int relayCount, TerminalBlockRow terminals, int usefulWidthMm)
        {
            var side = new CabinetSide { TotalAllocatedHeightMm = 40, TotalDinRailsCount = 0 };

            // Получаем плоский список всех необходимых физических блоков оборудования для задней панели
            var flatEquipmentList = new List<PlcComponent>();

            int remainingRelayWidth = relayCount * 16;
            while (remainingRelayWidth > 0)
            {
                int width = Math.Min(remainingRelayWidth, usefulWidthMm);
                flatEquipmentList.Add(new PlcComponent { Article = "FINDER", Name = "Блок реле", Type = "RELAY_BLOCK", WidthMm = width });
                remainingRelayWidth -= width;
            }

            int remainingTerminalWidth = (terminals.GreyTerminalsCount + terminals.BlueTerminalsCount + terminals.PeTerminalsCount) * 5;
            while (remainingTerminalWidth > 0)
            {
                int width = Math.Min(remainingTerminalWidth, usefulWidthMm);
                flatEquipmentList.Add(new PlcComponent { Article = "IEK_COL", Name = "Клеммный ряд", Type = "TERMINAL_BLOCK", WidthMm = width });
                remainingTerminalWidth -= width;
            }

            // Алгоритм ГОРИЗОНТАЛЬНОЙ укладки блоков на DIN-рейки задней панели
            int backBasketIdx = 0;
            var currentBasket = new PlcBasket { BasketIndex = backBasketIdx };

            foreach (var eq in flatEquipmentList)
            {
                int currentOccupiedWidth = currentBasket.Modules.Sum(m => m.WidthMm);

                // Проверяем: поместятся ли блоки вместе на ОДНУ горизонтальную DIN-рейку 580 мм
                if (currentOccupiedWidth + eq.WidthMm <= usefulWidthMm)
                {
                    currentBasket.Modules.Add(eq);
                }
                else
                {
                    // Место на текущей рейке задней панели закончилось! Сохраняем её по высоте
                    var node = new PlcNode();
                    node.Baskets.Add(currentBasket);
                    side.Nodes.Add(node);

                    side.TotalAllocatedHeightMm += 265; // Добавляем высоту шага (рейка + короб)
                    side.TotalDinRailsCount++;

                    // Переходим на следующую рейку задней панели (CARRIER BACK)
                    backBasketIdx++;
                    currentBasket = new PlcBasket { BasketIndex = backBasketIdx };
                    currentBasket.Modules.Add(eq);
                }
            }

            // Не забываем сохранить последний заполняемый ряд
            if (currentBasket.Modules.Any())
            {
                var node = new PlcNode();
                node.Baskets.Add(currentBasket);
                side.Nodes.Add(node);

                side.TotalAllocatedHeightMm += 265;
                side.TotalDinRailsCount++;
            }

            return side;
        }



        private PowerSupplyUnit CalculatePSU(int modulesCount)
        {
            double watts = (12 + (modulesCount * 6)) * 1.25;
            return watts switch
            {
                <= 15 => new PowerSupplyUnit { Article = "БП15Б-Д2-24", Name = "Блок питания 15Вт 24В", WidthMm = 36, PowerWatts = 15 },
                <= 30 => new PowerSupplyUnit { Article = "БП30Б-Д3-24", Name = "Блок питания 30Вт 24В", WidthMm = 54, PowerWatts = 30 },
                <= 60 => new PowerSupplyUnit { Article = "БП60Б-Д4-24", Name = "Блок питания 60Вт 24В", WidthMm = 72, PowerWatts = 60 },
                _ => new PowerSupplyUnit { Article = "БП120Б-Д9-24", Name = "Блок питания 120Вт 24В", WidthMm = 160, PowerWatts = 120 }
            };
        }

        private TerminalBlockRow CalculateCabinetTerminals(CabinetSide frontSide)
        {
            var terminals = new TerminalBlockRow();
            foreach (var node in frontSide.Nodes)
                foreach (var basket in node.Baskets)
                    foreach (var m in basket.Modules)
                    {
                        if (m.Type == "DI") { terminals.GreyTerminalsCount += m.DiChannels; terminals.BlueTerminalsCount += m.DiChannels; }
                        else if (m.Type == "DO") { terminals.GreyTerminalsCount += m.DoChannels; terminals.BlueTerminalsCount += m.DoChannels; }
                        else if (m.Type == "AI_Current") { terminals.GreyTerminalsCount += m.AiChannels; terminals.BlueTerminalsCount += m.AiChannels; terminals.PeTerminalsCount += m.AiChannels; }
                        else if (m.Type == "AI_Temp") { terminals.GreyTerminalsCount += (m.AiChannels * 2); terminals.BlueTerminalsCount += m.AiChannels; }
                        else if (m.Type == "AO") { terminals.GreyTerminalsCount += m.AoChannels; terminals.BlueTerminalsCount += m.AoChannels; terminals.PeTerminalsCount += m.AoChannels; }
                    }
            terminals.EndPlatesCount = (terminals.GreyTerminalsCount > 0 ? 1 : 0) + (terminals.BlueTerminalsCount > 0 ? 1 : 0) + (terminals.PeTerminalsCount > 0 ? 1 : 0);
            return terminals;
        }

        private (List<PlcComponent> PickedModules, int RemainingChannels) PickModules(int requiredChannels, IEnumerable<PlcComponent> modules)
        {
            var picked = new List<PlcComponent>();
            if (requiredChannels <= 0) return (picked, 0);
            var sorted = modules.OrderByDescending(m => GetChannelCount(m)).ToList();
            if (!sorted.Any()) return (picked, requiredChannels);
            int currentRemaining = requiredChannels;
            while (currentRemaining > 0)
            {
                int innerChannels = currentRemaining;
                var bestModule = sorted.FirstOrDefault(m => GetChannelCount(m) <= innerChannels) ?? sorted.First();
                picked.Add(bestModule);
                currentRemaining -= GetChannelCount(bestModule);
            }
            return (picked, currentRemaining);
        }

        private int GetChannelCount(PlcComponent m) => m.Type switch
        {
            "DI" => m.DiChannels,
            "DO" => m.DoChannels,
            "AI_Current" or "AI_Temp" => m.AiChannels,
            "AO" => m.AoChannels,
            _ => 1
        };
    }
}
