using System;
using System.Collections.Generic;
using System.Linq;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services
{
    public class PlcCalculationService
    {
        public List<PlcBasket> GeneratePlcBaskets(
            SignalRequirement signals,
            PlcComponent selectedCpu,
            List<PlcComponent> availableModules,
            PlcComponent? busCoupler = null,
            int maxSlotsPerBasket = 8)
        {
            var baskets = new List<PlcBasket>();

            // 1. Расчет необходимых каналов с учетом резерва по ГОСТ
            double coeff = 1 + (signals.ReservePercent / 100);
            int reqDi = (int)Math.Ceiling(signals.TotalDI * coeff);
            int reqDo = (int)Math.Ceiling(signals.TotalDO * coeff);

            // Разделяем аналоговый ввод по типам модулей
            int reqAiStandard = (int)Math.Ceiling((signals.AiCurrent2W + signals.AiCurrent4W + signals.AiVoltage) * coeff);
            int reqAiTemp = (int)Math.Ceiling((signals.AiRtd + signals.AiTc) * coeff);

            int reqAo = (int)Math.Ceiling(signals.TotalAO * coeff);

            // 2. Вычитаем встроенные каналы процессора
            reqDi -= selectedCpu.DiChannels;
            reqDo -= selectedCpu.DoChannels;
            reqAiStandard -= selectedCpu.AiChannels;
            reqAo -= selectedCpu.AoChannels;

            // 3. Формируем пул необходимых модулей расширения (без использования ref)
            var requiredModules = new List<PlcComponent>();

            var (diMods, _) = PickModules(reqDi, availableModules.Where(m => m.Type == "DI"));
            requiredModules.AddRange(diMods);

            var (doMods, _) = PickModules(reqDo, availableModules.Where(m => m.Type == "DO"));
            requiredModules.AddRange(doMods);

            var (aiStdMods, _) = PickModules(reqAiStandard, availableModules.Where(m => m.Type == "AI_Current"));
            requiredModules.AddRange(aiStdMods);

            var (aiTempMods, _) = PickModules(reqAiTemp, availableModules.Where(m => m.Type == "AI_Temp"));
            requiredModules.AddRange(aiTempMods);

            var (aoMods, _) = PickModules(reqAo, availableModules.Where(m => m.Type == "AO"));
            requiredModules.AddRange(aoMods);

            // 4. Распределение по корзинам
            int currentBasketIndex = 0;
            var currentBasket = new PlcBasket { BasketIndex = currentBasketIndex };
            currentBasket.Modules.Add(selectedCpu);

            foreach (var module in requiredModules)
            {
                if (currentBasket.TotalSlotsUsed + module.SlotsOccupied <= maxSlotsPerBasket)
                {
                    currentBasket.Modules.Add(module);
                }
                else
                {
                    baskets.Add(currentBasket);

                    currentBasketIndex++;
                    currentBasket = new PlcBasket { BasketIndex = currentBasketIndex };
                    if (busCoupler != null) currentBasket.BusCouplerArticle = busCoupler.Article;

                    currentBasket.Modules.Add(module);
                }
            }

            if (currentBasket.Modules.Count > 0) baskets.Add(currentBasket);
            return baskets;
        }

        // Исправленный метод: принимает обычный int и возвращает кортеж с остатком каналов
        private (List<PlcComponent> PickedModules, int RemainingChannels) PickModules(int requiredChannels, IEnumerable<PlcComponent> modules)
        {
            var picked = new List<PlcComponent>();
            if (requiredChannels <= 0) return (picked, 0);

            var sorted = modules.OrderByDescending(m => GetChannelCount(m)).ToList();
            if (!sorted.Any()) return (picked, requiredChannels);

            // Копируем во локальную переменную, чтобы безопасно менять внутри цикла
            int currentRemaining = requiredChannels;

            while (currentRemaining > 0)
            {
                // Локальная копия для лямбда-выражения
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
            _ => 0
        };
    }
}
