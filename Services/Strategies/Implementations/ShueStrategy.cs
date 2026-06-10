using System;
using System.Collections.Generic;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services.Strategies.Implementations;

public class ShueStrategy : ICabinetStrategy
{
    private readonly ShuvConfigLoader _loader;

    public ShueStrategy(ShuvConfigLoader loader) => _loader = loader;

    public string CabinetType => "Шкаф управления электрообогревом (ШУЭ)";

    public List<SelectedComponent> CalculateComponents(UiConfigInput input)
    {
        var components = new List<SelectedComponent>();

        // 1. Общие компоненты из JSON
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "Configs", "shue-strategy.json");
        var config = _loader.Load(path);

        if (config?.Rules != null)
        {
            var counters = new Dictionary<string, int>();
            foreach (var device in config.Rules)
            {
                if (!CheckCondition(device.Condition, input)) continue;
                string designation = AutoNumber(device.Designation, counters);

                components.Add(new SelectedComponent
                {
                    Designation = designation,
                    Article = device.Article,
                    Vendor = device.DefaultSupplier,
                    Description = device.Description,
                    Quantity = device.Quantity
                });
            }
            // База типоразмеров навесных шкафов (H, W, D, IP)
            var wallMountedCabinets = new Dictionary<(int H, int W, int D, string IP), (string Article, string Description)>
            {
                // IP54
                { (400, 300, 220, "IP54"), ("YKM40-01-54", "Корпус металлический ЩМП-1-0 (400х300х220 мм) У2 IP54") },
                { (500, 400, 220, "IP54"), ("YKM40-02-54", "Корпус металлический ЩМП-2-0 (500х400х220мм) У2 IP54") },
                { (600, 600, 250, "IP54"), ("YKM40-662-54", "Корпус металлический ЩМП-6.6.2-0 (600х600х250 мм) У2 IP54") },
                { (800, 650, 250, "IP54"), ("YKM40-04-54", "Корпус металлический ЩМП-4-0 (800х650х250 мм) У2 IP54") },
                { (1000, 650, 285, "IP54"), ("YKM40-05-54", "Корпус металлический ЩМП-5-0 (1000х650х285 мм) У2 IP54") },
                { (1200, 750, 300, "IP54"), ("YKM40-06-54", "Корпус металлический ЩМП-6-0 (1200х750х300 мм) У2 IP54") },
                { (1400, 650, 285, "IP54"), ("YKM40-07-54", "Корпус металлический ЩМП-7-0 (1400х650х285 мм) У2 IP54") },
    
                // IP66
                { (400, 300, 200, "IP66"), ("TI5-10-P-040-030-020-66", "TITAN 5 Корпус металлический ЩМП-40.30.20 УХЛ1 IP66") },
                { (600, 400, 250, "IP66"), ("TI5-10-P-060-040-025-66", "TITAN 5 Корпус металлический ЩМП-60.40.25 УХЛ1 IP66") },
                { (800, 600, 300, "IP66"), ("TI5-10-P-080-060-030-66", "TITAN 5 Корпус металлический ЩМП-80.60.30 УХЛ1 IP66") },
                { (1000, 800, 300, "IP66"), ("TI5-10-P-100-080-030-66", "TITAN 5 Корпус металлический ЩМП-100.80.30 УХЛ1 IP66") },
                { (1200, 800, 400, "IP66"), ("TI5-10-P-120-080-040-66", "TITAN 5 Корпус металлический ЩМП-100.80.30 УХЛ1 IP66") },
                { (1400, 800, 300, "IP66"), ("TI5-10-N-140-080-030-66", "TITAN 5 Корпус металлический ЩМП-140.80.30 УХЛ1 IP66") },
            };

            // Конструктив шкафа
            if (input.BaseConfig != null)
            {
                if (input.BaseConfig.MountType == "Навесной")
                {
                    components.Add(new SelectedComponent
                    {
                        Designation = "DIN - рейка",
                        Article = "TF-DN25-0200",
                        Vendor = "IEK",
                        Description = "DIN - рейка, 2 м",
                        Quantity = 1
                    });
                    components.Add(new SelectedComponent
                    {
                        Designation = "Кабель-канал",
                        Article = "CKM50-025-040-1-K03",
                        Vendor = "IEK",
                        Description = "Кабель-канал перфорированный 25х40",
                        Quantity = 2
                    });
                }

                var key = (input.BaseConfig.Height, input.BaseConfig.Width, input.BaseConfig.Depth, input.BaseConfig.IpRating ?? "IP54");

                if (wallMountedCabinets.TryGetValue(key, out var cabinet))
                {
                    components.Add(new SelectedComponent
                    {
                        Designation = "Шкаф",
                        Article = cabinet.Article,
                        Vendor = "IEK",
                        Description = cabinet.Description,
                        Quantity = 1
                    });
                }
                else
                {
                    components.Add(new SelectedComponent
                    {
                        Designation = "Шкаф",
                        Article = "",
                        Vendor = "По проекту",
                        Description = $"Шкаф навесной {input.BaseConfig.Height}×{input.BaseConfig.Width}×{input.BaseConfig.Depth} мм {input.BaseConfig.IpRating}",
                        Quantity = 1
                    });
                }

                // Обогрев шкафа
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

        }

        // 2. Отходящие линии из таблицы
        if (input.HeatingLines != null)
        {
            int kmCounter = 0;
            int tcCounter = 0;

            foreach (var line in input.HeatingLines)
            {
                if (!line.IsEnabled) continue;

                string article;
                string description;

                // Диф автомат 2P
                if (line.HasRCD)
                {
                    
                    article = line.Current switch
                    {
                        6 => "D63N26E6C30",
                        10 => "D63N26E16C30",
                        16 => "D63N26E16C30",
                        20 => "D63N26E20C30",
                        25 => "D63N26E25C30",
                        32 => "D63N26E32C30",
                        40 => "D63N26E40C30",
                        50 => "D63N26E50C30",
                        63 => "D63N26E63C30",
                        _ => $"D63N26E{line.Current}C30"
                    };
                    description = $"Дифавтомат {line.Poles}P C{line.Current} А 30мА {line.IkZ} кА";
                }
                // Автоматы
                else
                {
                    article = line.Poles switch
                    {
                        // 1P
                        1 => line.Current switch
                        {
                            1 => "M636101C",
                            2 => "M636102C",
                            3 => "M636103C",
                            4 => "M636104C",
                            6 => "M636106C",
                            10 => "M636110C",
                            16 => "M636116C",
                            20 => "M636120C",
                            25 => "M636125C",
                            32 => "M636132C",
                            40 => "M636140C",
                            50 => "M636150C",
                            63 => "M636163C",
                            _ => $"M6361{line.Current}C"
                        },
                        // 2P
                        2 => line.Current switch
                        {
                            1 => "M636201C",
                            2 => "M636202C",
                            3 => "M636203C",
                            4 => "M636204C",
                            6 => "M636206C",
                            10 => "M636210C",
                            16 => "M636216C",
                            20 => "M636220C",
                            25 => "M636225C",
                            32 => "M636232C",
                            40 => "M636240C",
                            50 => "M636250C",
                            63 => "M636263C",
                            _ => $"M6362{line.Current}C"
                        },
                        // 3P
                        3 => line.Current switch
                        {
                            1 => "M636301C",
                            2 => "M636302C",
                            3 => "M636303C",
                            4 => "M636304C",
                            6 => "M636306C",
                            10 => "M636310C",
                            16 => "M636316C",
                            20 => "M636320C",
                            25 => "M636325C",
                            32 => "M636332C",
                            40 => "M636340C",
                            50 => "M636350C",
                            63 => "M636363C",
                            _ => $"M6363{line.Current}C"
                        },
                        _ => $"M6361{line.Current}C"
                    };
                    description = $"Автоматический выключатель {line.Poles}P C{line.Current} А {line.IkZ} кА";
                }

                components.Add(new SelectedComponent
                {
                    Designation = line.Designation,
                    Article = article,
                    Vendor = "EKF",
                    Description = description,
                    Quantity = 1
                });

                if (line.HasContactor)
                {
                    kmCounter++;
                    string contactorArticle = line.Current switch
                    {
                        <= 16 => "KM-2-16-20",
                        <= 25 => "KM-2-25-20",
                        <= 32 => "KM-2-32-20",
                        <= 40 => "KM-2-40-20",
                        <= 63 => "KM-2-63-20",
                        _ => "KM-2-63-20"
                    };

                    components.Add(new SelectedComponent
                    {
                        Designation = $"KM{kmCounter}",
                        Article = contactorArticle,
                        Vendor = "EKF",
                        Description = $"Контактор {line.Poles}P {line.Current} А 230 В",
                        Quantity = 1
                    });
                }

                if (line.HasThermostat)
                {
                    tcCounter++;
                    components.Add(new SelectedComponent
                    {
                        Designation = $"KK{tcCounter}",
                        Article = "EKRT-820M",
                        Vendor = "EKF",
                        Description = "Реле температуры с дисплеем RT-820M (-25....+130 С)",
                        Quantity = 1
                    });
                }
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

    private static bool CheckCondition(string condition, UiConfigInput input)
    {
        if (condition == "Always") return true;
        return condition switch
        {
            "Линия обогрева" => input.HeatingLines?.Any(l => l.IsEnabled) == true,
            _ => false
        };
    }

    private static string AutoNumber(string baseDesignation, Dictionary<string, int> counters)
    {
        if (!counters.ContainsKey(baseDesignation)) counters[baseDesignation] = 0;
        counters[baseDesignation]++;
        return $"{baseDesignation}{counters[baseDesignation]}";
    }
}