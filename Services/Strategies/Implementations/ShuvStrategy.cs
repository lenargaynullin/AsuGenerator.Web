using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsuGenerator.Web.Models;
using AsuGenerator.Web.Services.Strategies; // <-- ДОБАВЛЕНО ДЛЯ СВЯЗИ С ИНТЕРФЕЙСОМ

namespace AsuGenerator.Web.Services.Strategies.Implementations;

public class ShuvStrategy(ShuvConfigLoader loader) : ICabinetStrategy
{
    private readonly ShuvConfigLoader _loader = loader;

    public string CabinetType => "Шкаф управления вентиляцией (ШУВ)";

    public List<SelectedComponent> CalculateComponents(UiConfigInput input)
    {
        try
        {
            if (input == null)
                return [new() { Designation = "ERR", Article = "ERR", Description = "input == null", Vendor = "ERR" }];

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "Configs", "shuv-strategy.json");
            var config = _loader.Load(path);

            if (config?.Rules == null || config.Rules.Count == 0)
                return [new() { Designation = "ERR", Article = "ERR", Description = "Rules пуст", Vendor = "ERR" }];

            var components = new List<SelectedComponent>();
            var counters = new Dictionary<string, int>();
            var contextIndexes = new Dictionary<string, int>();

            foreach (var device in config.Rules)
            {
                if (!CheckCondition(device.Condition, input))
                    continue;

                string raw = device.Designation;
                string baseCode = raw.Contains('_') ? raw.Split('_')[0] : raw;
                bool isAccessory = raw.Contains('_');
                string contextKey = $"{baseCode}_{device.Condition}";

                // Инициализируем переменную перед поиском
                int index;

                // Выполняем поиск ВСЕГО ОДИН РАЗ через TryGetValue
                if (isAccessory && contextIndexes.TryGetValue(contextKey, out var existingIndex))
                {
                    index = existingIndex;
                }
                else
                {
                    // Оптимизируем счетчики: если ключа нет, TryGetValue вернет false и запишет 0 в currentCounter
                    if (!counters.TryGetValue(baseCode, out var currentCounter))
                    {
                        currentCounter = 0;
                    }

                    currentCounter++;
                    counters[baseCode] = currentCounter;
                    index = currentCounter;

                    if (!isAccessory)
                    {
                        contextIndexes[contextKey] = index;
                    }
                }


                string designation = FormatDesignation(raw, baseCode, index);

                components.Add(new SelectedComponent
                {
                    Designation = designation,
                    Article = device.Article,
                    Vendor = device.DefaultSupplier,
                    Description = device.Description,
                    Quantity = device.Quantity
                });
            }
            // База типоразмеров навесных шкафов
            var wallMountedCabinets = new Dictionary<(int H, int W, int D), (string Article, string Description)>
            {
                { (600, 400, 250), ("MES 60.40.25", "Шкаф компактный распределительный (с монтажной панелью)") },
                { (800, 600, 300), ("MES 80.60.30", "Шкаф компактный распределительный (с монтажной панелью)") },
                { (1000, 800, 300), ("MES 100.80.30", "Шкаф компактный распределительный (с монтажной панелью)") },
                { (1200, 800, 400), ("MES 120.80.40", "Шкаф компактный распределительный (с монтажной панелью)") },
            };

            // Конструктив шкафа / Общие параметры шкафа
            if (input.BaseConfig != null)
            {
                // 1 Навесной 
                if (input.BaseConfig.MountType == "Навесной")
                {
                    components.Add(new SelectedComponent
                    {
                        Designation = "Скобы",
                        Article = "WB 8",
                        Vendor = "ПРОВЕНТО",
                        Description = "Скобы для монтажа на стене",
                        Quantity = 4
                    });
                    components.Add(new SelectedComponent
                    {
                        Designation = "DIN - рейка",
                        Article = "DR 15.2000",
                        Vendor = "ПРОВЕНТО",
                        Description = "DIN - рейка, 2 м",
                        Quantity = 1
                    });
                }
                var key = (input.BaseConfig.Height, input.BaseConfig.Width, input.BaseConfig.Depth);

                if (wallMountedCabinets.TryGetValue(key, out var cabinet))
                {
                    components.Add(new SelectedComponent
                    {
                        Designation = "Шкаф",
                        Article = cabinet.Article,
                        Vendor = "ПРОВЕНТО",
                        Description = cabinet.Description,
                        Quantity = 1
                    });
                }
                else
                {
                    // Размер не из типовой базы — шкаф по проекту
                    components.Add(new SelectedComponent
                    {
                        Designation = "Шкаф",
                        Article = "",
                        Vendor = "По проекту",
                        Description = $"Шкаф навесной {input.BaseConfig.Height}×{input.BaseConfig.Width}×{input.BaseConfig.Depth} мм {input.BaseConfig.IpRating}",
                        Quantity = 1
                    });
                }

                // Обогреватель
                if (input.BaseConfig.HasHeater)
                {
                    components.Add(new SelectedComponent
                    {
                        Designation = "EK",
                        Article = "YCE-CRE-050-65",
                        Vendor = "IEK",
                        Description = "Обогреватель на DIN-рейку 50Вт",
                        Quantity = 1
                    });
                }
            }
            // Группировка одинаковых артикулов
            var grouped = new List<SelectedComponent>();

            foreach (var comp in components)
            {
                var existing = grouped.FirstOrDefault(g => g.Article == comp.Article && g.Vendor == comp.Vendor);
                if (existing != null)
                {
                    existing.Quantity += comp.Quantity;
                    // Добавляем обозначение через запятую, если его ещё нет
                    if (!existing.Designation.Contains(comp.Designation))
                        existing.Designation += ", " + comp.Designation;
                }
                else
                {
                    grouped.Add(new SelectedComponent
                    {
                        Designation = comp.Designation,
                        Article = comp.Article,
                        Vendor = comp.Vendor,
                        Description = comp.Description,
                        Quantity = comp.Quantity
                    });
                }
            }

            return grouped;
        }
        catch (Exception ex)
        {
            return [new() { Designation = "ERR", Article = "ERR", Description = ex.Message, Vendor = ex.GetType().Name }];
        }
    }

    private static bool CheckCondition(string condition, UiConfigInput input)
    {
        if (condition == "Always" || condition == "Всегда") return true;

        return condition switch
        {
            "Приточный вентилятор" or "SupplyFan" => input.SupplyFan,
            "SupplyFan && HasVfd" => input.SupplyFan && input.HasVfd,
            "SupplyFan && !HasVfd" => input.SupplyFan && !input.HasVfd,
            "SupplyFan && HasVfd && !HasHeaterPump" => input.SupplyFan && input.HasVfd && !input.HasHeaterPump,
            "Насос калорифера" or "HasHeaterPump" => input.HasHeaterPump,
            "Насос увлажнителя" or "HasHumidifierPump" => input.HasHumidifierPump,
            "Заслонка приточная" or "HasDamper" => input.HasDamper,
            "3-х ходовой клапан" or "Has3WayValve" => input.Has3WayValve,
            "Авария увлажнитель" => input.HasHumidifierPump,
            "Авария блок кондиционера" => input.HasAirConditioner,
            "Пуск блок кондиционера" => input.HasAirConditioner,
            "Сигнал аварии" => input.SupplyFan,
            "DI на ЧП ПВ" => input.SupplyFan,
            "Т наружная" => input.SupplyFan,
            "Т воды" => input.HasHeaterPump,
            "ПВТ100" => input.SupplyFan,
            "Диспетчеризация" => input.HasDispatching,
            _ => false
        };
    }

    private static string FormatDesignation(string raw, string baseCode, int index)
    {
        if (!raw.Contains('_'))
            return $"{baseCode}{index}";

        return raw switch
        {
            _ when raw.EndsWith("_Socket") => $"Колодка для {baseCode}{index}",
            _ when raw.EndsWith("_Clip") => $"Зажим для {baseCode}{index}",
            _ when raw.EndsWith("_Led") => $"Индикатор {baseCode}{index}",
            _ when raw.EndsWith("_Block") => $"Контакт для {baseCode}{index}",
            _ when raw.EndsWith("_End") => $"Заглушка {baseCode}",
            _ when raw.EndsWith("_Common") => $"{baseCode} (Общие)",
            _ => $"{baseCode}{index}"
        };
    }
}