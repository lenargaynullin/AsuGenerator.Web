using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AsuGenerator.Web.Models;
using Microsoft.AspNetCore.Hosting;
using static AsuGenerator.Web.Components.Pages.Configurator;

namespace AsuGenerator.Web.Services
{
    public class UniversalCalculationEngine
    {
        private readonly IWebHostEnvironment _env;

        public UniversalCalculationEngine(IWebHostEnvironment env)
        {
            _env = env;
        }

        public List<SelectedComponent> CalculateSpecification(BaseCabinetConfig config, string dimensions, List<HeatingLine> lines, List<TerminalRow> terminals)
        {
            var finalSpec = new List<SelectedComponent>();

            // ----------------------------------------------------
            // ШАГ 1 LOGIC: ПОДБОР ОБОЛОЧКИ И АКСЕССУАРОВ ПРОВЕНТО
            // ----------------------------------------------------
            string cabinetJsonPath = Path.Combine(_env.WebRootPath, "Configs", "cabinet-base.json");
            int cabinetHeight = 0, cabinetWidth = 0, cabinetDepth = 0;

            // Парсим габариты шкафа в инты, чтобы использовать их для артикулов аксессуаров
            var dims = dimensions.Split('×');
            if (dims.Length == 3)
            {
                int.TryParse(dims[0], out cabinetHeight);
                int.TryParse(dims[1], out cabinetWidth);
                int.TryParse(dims[2], out cabinetDepth);
            }

            if (File.Exists(cabinetJsonPath))
            {
                var cabinets = JsonSerializer.Deserialize<List<JsonCabinetItem>>(File.ReadAllText(cabinetJsonPath));
                var matchedCabinet = cabinets?.FirstOrDefault(c =>
                    c.Manufacturer == config.Manufacturer &&
                    c.MountType == config.MountType &&
                    c.IpRating == config.IpRating &&
                    c.Dimensions == dimensions);

                if (matchedCabinet != null)
                {
                    finalSpec.Add(new SelectedComponent { Designation = "Шкаф", Vendor = config.Manufacturer, Description = matchedCabinet.Name, Article = matchedCabinet.Article, Quantity = 1 });
                }
            }

            // АВТОПОДБОР АКСЕССУАРОВ ПОД ГАБАРИТЫ КОРПУСА ПРОВЕНТО
            if (config.Manufacturer == "ПРОВЕНТО")
            {
                // 1. Карман для документации (зависит от ширины двери шкафа)
                if (config.HasPocket && cabinetWidth > 0 && config.MountType == "Напольный")
                {
                    if (cabinetWidth == 400)
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Карман для документации",
                            Article = $"DP 40 M",
                            Quantity = 1
                        });
                    }
                    ;
                    if (cabinetWidth == 500 || cabinetWidth == 1000)
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Карман для документации",
                            Article = $"DP 50 M",
                            Quantity = 1
                        });
                    }
                    ;
                    if (cabinetWidth == 600 || cabinetWidth == 1200)
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Карман для документации",
                            Article = $"DP 60 M",
                            Quantity = 1
                        });
                    }
                    ;
                    if (cabinetWidth == 800)
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Карман для документации",
                            Article = $"DP 80 M",
                            Quantity = 1
                        });
                    }
                    ;
                }
                else
                { // Карман пластиковый
                    finalSpec.Add(new SelectedComponent
                    {
                        Designation = "",
                        Vendor = "ПРОВЕНТО",
                        Description = $"Карман для документации",
                        Article = $"DP 40 P",
                        Quantity = 1
                    });
                }

                // 2. Ручка двери с замком (для напольных линеек MPS)
                if (config.HasDoorHandle && config.MountType == "Напольный")
                {
                    finalSpec.Add(new SelectedComponent
                    {
                        Designation = "",
                        Vendor = "ПРОВЕНТО",
                        Description = "Ручка поворотная с цилиндром",
                        Article = "LH 1C.Z",
                        Quantity = 1
                    });
                }

                // 3. Подставка (Добавить Кнопку-переключатель в конфигураторе)
                if (config.MountType == "Напольный" && config.HasSupportBase && cabinetHeight > 0 && cabinetDepth > 0)
                {
                    if (cabinetWidth == 1000) finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Подставка шкафа", Article = "SH 50 D", Quantity = 1 });
                    if (cabinetWidth == 600 || cabinetWidth == 1200) finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Подставка шкафа", Article = "SH 60 D", Quantity = 1 });
                    if (cabinetWidth == 800) finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Подставка шкафа", Article = "SH 80 D", Quantity = 1 });
                }

                // 4. Светильник (Добавить Кнопку-переключатель в конфигураторе)

                if (config.HasLighting == true) // Светильник завяжем на тип шкафа или можно добавить как чекбокс config.HasLighting
                {
                    finalSpec.Add(new SelectedComponent
                    {
                        Designation = "HL",
                        Vendor = "ПРОВЕНТО",
                        Description = "Светильник светодиодный, ~220В",
                        Article = "LA 5 LED",
                        Quantity = 1
                    });
                }

                // 5. Разборный цоколь H=100мм или 200мм (состоит из передних PPL и боковых PPB элементов)
                if (config.MountType == "Напольный" && config.PlinthHeight != "Нет" && cabinetWidth > 0 && cabinetDepth > 0)
                {
                    string heightDigits = config.PlinthHeight.Contains("100") ? "100" : "200";

                    // Высота 100 мм
                    if (heightDigits == "100")
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Передние и задние элементы цоколя высота {heightDigits} мм",
                            Article = $"ZA {cabinetWidth / 10}.00",
                            Quantity = 1
                        });

                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Боковые элементы цоколя высота {heightDigits} мм",
                            Article = $"ZA 00.{cabinetDepth / 10}",
                            Quantity = 1
                        });
                    }
                    ;

                    // Высота 200 мм
                    if (heightDigits == "200")
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Передние и задние элементы цоколя высота {heightDigits} мм",
                            Article = $"ZA {cabinetWidth / 10}.00 H",
                            Quantity = 1
                        });

                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Боковые элементы цоколя высота {heightDigits} мм",
                            Article = $"ZA 00.{cabinetDepth / 10} H",
                            Quantity = 1
                        });
                    }
                    ;
                }

                // 6. Боковые панели каркаса (для линейки MPS всегда закладываются парой)
                if (config.MountType == "Напольный" && cabinetHeight > 0 && cabinetDepth > 0)
                {
                    finalSpec.Add(new SelectedComponent
                    {
                        Designation = "",
                        Vendor = "ПРОВЕНТО",
                        Description = $"Боковые панели (высота {cabinetHeight} мм, глубина {cabinetDepth} мм)",
                        Article = $"SP {cabinetHeight / 10}.{cabinetDepth / 10}",
                        Quantity = 2 // Строго две штуки (левая и правая) по ГОСТу
                    });
                }
                // 7. Рым болт (Добавить Кнопку-переключатель в конфигураторе)
                if (config.MountType == "Напольный" && config.HasEyebolts && cabinetHeight > 0 && cabinetDepth > 0)
                {
                    finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Рым болт (комплект 4 шт.)", Article = "LE 12", Quantity = 4 });
                }
                // 8. Концевой выключатель (Добавить Кнопку-переключатель в конфигураторе)
                if (config.MountType == "Напольный" && config.HasLimitSwitch && cabinetHeight > 0 && cabinetDepth > 0)
                {
                    finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Концевой выключатель двери шкафа", Article = "SW 01", Quantity = 1 });
                }
                // 9. Рейка для глухой двери (Связано с чекбоксом UI)
                if (config.MountType == "Напольный" && config.HasDoorRails && cabinetHeight > 0 && cabinetDepth > 0)
                {
                    if (cabinetWidth == 400) finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Рейка для глухой двери", Article = "VB 40 G", Quantity = 1 });
                    if (cabinetWidth == 500 || cabinetWidth == 1000) finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Рейка для глухой двери", Article = "VB 50 G", Quantity = 1 });
                    if (cabinetWidth == 600 || cabinetWidth == 1200) finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Рейка для глухой двери", Article = "VB 60 G", Quantity = 1 });
                    if (cabinetWidth == 800) finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Рейка для глухой двери", Article = "VB 80 G", Quantity = 1 });
                }
                // 10. Потолочная панель (Связано с чекбоксом UI)
                if (config.MountType == "Напольный" && config.HasRoofCablePanel && cabinetHeight > 0 && cabinetDepth > 0)
                {
                    finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Потолочная панель с перфорацией и вводом для кабелей", Article = $"R {cabinetWidth / 10}.{cabinetDepth / 10} PK", Quantity = 1 });
                }
                // 11. Монтажная панель (Связано с чекбоксом UI)
                if (config.MountType == "Напольный" && config.HasMountPanel && cabinetHeight > 0 && cabinetDepth > 0)
                {
                    finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Монтажная панель внутренняя", Article = $"MP {cabinetWidth / 10}.{cabinetDepth / 10}", Quantity = 1 });
                }
                // 12. Дверь (Связано с чекбоксом UI)
                if (config.MountType == "Напольный" && config.HasCabinetDoor && cabinetHeight > 0 && cabinetDepth > 0)
                {
                    finalSpec.Add(new SelectedComponent { Designation = "", Vendor = "ПРОВЕНТО", Description = "Дверь передняя глухая", Article = $"D {cabinetWidth / 10}.{cabinetDepth / 10}", Quantity = 1 });
                }
                // 13. Подбор вентилятора Провенто с учетом выбранного на UI количества
                if (!string.IsNullOrEmpty(config.FanModel) && config.FanModel != "Нет")
                {
                    finalSpec.Add(new SelectedComponent
                    {
                        Designation = "",
                        Vendor = "ПРОВЕНТО",
                        Description = $"Вентилятор фильтрующий 230 В",
                        Article = config.FanModel,
                        Quantity = config.FanQuantity // ИСПРАВЛЕНО: Теперь подставляется живое количество 1 или 2!
                    });

                    // Инженерная фишка: Выпускной фильтр (решетка) всегда заказывается в таком же количестве!
                    if (config.FanModel == "FA 12.230")
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Решетка с фильтром",
                            Article = "FF 12 D",
                            Quantity = config.FanQuantity
                        });
                    }
                    if (config.FanModel == "FA 13.230")
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Решетка с фильтром",
                            Article = "FF 13 D",
                            Quantity = config.FanQuantity
                        });
                    }
                    if (config.FanModel == "FA 15.230")
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Решетка с фильтром",
                            Article = "FF 15 D",
                            Quantity = config.FanQuantity
                        });
                    }
                    if (config.FanModel == "FA 08.230")
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Решетка с фильтром",
                            Article = "FF 08 D",
                            Quantity = config.FanQuantity
                        });
                    }
                    if (config.FanModel == "FA 20.230")
                    {
                        finalSpec.Add(new SelectedComponent
                        {
                            Designation = "",
                            Vendor = "ПРОВЕНТО",
                            Description = $"Решетка с фильтром",
                            Article = "FF 20 D",
                            Quantity = config.FanQuantity
                        });
                    }
                }
            }

            // ----------------------------------------------------
            // 2. ШАГ 2: УМНОЕ КВАНТОВАНИЕ КАНАЛОВ ОВЕН (ПР200 vs ПЛК210)
            // ----------------------------------------------------
            string plcJsonPath = Path.Combine(_env.WebRootPath, "Configs", "plc-base.json");
            if (File.Exists(plcJsonPath))
            {
                var plcDb = JsonSerializer.Deserialize<List<JsonPlcItem>>(File.ReadAllText(plcJsonPath));
                string targetPower = config.PlcPower == "220" ? "230V" : "24V";

                // Используем оператор switch для жесткого разделения логики подбора
                switch (config.PlcType)
                {
                    // ==========================================
                    // ВЕТКА А: ЛОГИКА ДЛЯ ПРОГРАММИРУЕМОГО РЕЛЕ ПР200
                    // ==========================================
                    case "ПР200":
                        var basePlc = plcDb?.FirstOrDefault(p => p.PlcType == "ПР200" && p.PowerSupply == targetPower);
                        if (basePlc != null)
                        {
                            finalSpec.Add(new SelectedComponent { Designation = "A1", Vendor = "ОВЕН", Description = basePlc.Name, Article = basePlc.Article, Quantity = 1 });

                            int remainingDi = Math.Max(0, config.DiCount - basePlc.DiCount);
                            int remainingDo = Math.Max(0, config.DoCount - basePlc.DoCount);
                            int remainingAi = Math.Max(0, config.AiCount - basePlc.AiCount);
                            int remainingAo = Math.Max(0, config.AoCount - basePlc.AoCount);

                            int moduleCounter = 2;
                            int addedPrmModulesCount = 0;
                            bool isLimitExceeded = false;

                            void AddPrmModule(JsonPlcItem module)
                            {
                                if (module == null) return;
                                if (addedPrmModulesCount < 2)
                                {
                                    finalSpec.Add(new SelectedComponent { Designation = $"A{moduleCounter++}", Vendor = "ОВЕН", Description = module.Name, Article = module.Article, Quantity = 1 });
                                    addedPrmModulesCount++;
                                }
                                else { isLimitExceeded = true; }
                            }

                            if (remainingAo > 0)
                            {
                                int count = (int)Math.Ceiling((double)remainingAo / 2);
                                var mod = plcDb?.FirstOrDefault(p => p.Article.StartsWith("ПРМ") && p.PowerSupply == targetPower && p.AoCount == 2);
                                for (int i = 0; i < count; i++) { if (addedPrmModulesCount >= 2) { isLimitExceeded = true; break; } AddPrmModule(mod); if (mod != null) remainingAi = Math.Max(0, remainingAi - mod.AiCount); }
                            }
                            if (remainingAi > 0 && !isLimitExceeded)
                            {
                                int count = (int)Math.Ceiling((double)remainingAi / 4);
                                var mod = plcDb?.FirstOrDefault(p => p.Article.StartsWith("ПРМ") && p.PowerSupply == targetPower && p.AiCount == 4);
                                for (int i = 0; i < count; i++) { if (addedPrmModulesCount >= 2) { isLimitExceeded = true; break; } AddPrmModule(mod); if (mod != null) remainingDo = Math.Max(0, remainingDo - mod.DoCount); }
                            }
                            while ((remainingDi > 0 || remainingDo > 0) && !isLimitExceeded)
                            {
                                var mod = plcDb?.FirstOrDefault(p => p.Article.EndsWith(".1") && p.PowerSupply == targetPower);
                                if (mod != null) { AddPrmModule(mod); remainingDi = Math.Max(0, remainingDi - mod.DiCount); remainingDo = Math.Max(0, remainingDo - mod.DoCount); }
                                else break;
                            }

                            if (isLimitExceeded || remainingDi > 0 || remainingDo > 0 || remainingAi > 0 || remainingAo > 0)
                            {
                                var firstPlc = finalSpec.FirstOrDefault(c => c.Designation == "A1");
                                if (firstPlc != null) firstPlc.Description += " ⚠️ ПРЕВЫШЕН ЛИМИТ ШИНЫ ПР200 (поддерживается макс. 2 модуля ПРМ)!";
                            }
                        }
                        break;

                    // ==========================================
                    // ВЕТКА Б: СВЕРХУМНЫЙ ПОДБОР МОДИФИКАЦИИ ПЛК210 И МОДУЛЕЙ
                    // ==========================================
                    case "ПЛК210":
                        if (plcDb != null && plcDb.Any())
                        {
                            // 1. УМНЫЙ ПОДБОР БАЗОВОГО БЛОКА ПЛК210
                            // Сортируем базу так, чтобы найти контроллер, который максимально близко покрывает ТЗ по входам/выходам
                            // Если у вас на UI выбран тип интерфейса (например, 01, 02, 11, 12), можно искать по артикулу. 
                            // Но алгоритмический подбор выберет лучшую модель сам:
                            var availablePlcs = plcDb.Where(p => p.PlcType == "ПЛК210" && p.Manufacturer == "ОВЕН").ToList();

                            JsonPlcItem bestPlc = null;

                            // Если инженер вбил аналоговые сигналы, приоритет моделям ПЛК210-04 или ПЛК210-14 (у них есть 4xAI)
                            if (config.AiCount > 0)
                            {
                                bestPlc = availablePlcs.FirstOrDefault(p => p.AiCount > 0);
                            }

                            // Если аналоговых нет или модель не найдена, ищем по максимальному совпадению дискретных входов
                            if (bestPlc == null)
                            {
                                bestPlc = availablePlcs
                                    .OrderByDescending(p => p.DiCount >= config.DiCount) // Сначала те, где входов хватает
                                    .ThenBy(p => Math.Abs(p.DiCount - config.DiCount))   // Минимизируем избыточность
                                    .FirstOrDefault();
                            }

                            // Защита: если по хитрым фильтрам не нашли, берем самый первый ПЛК210 из вашего JSON
                            if (bestPlc == null)
                            {
                                bestPlc = availablePlcs.FirstOrDefault();
                            }

                            if (bestPlc != null)
                            {
                                // Добавляем оптимальный ПЛК в спецификацию по ГОСТ
                                finalSpec.Add(new SelectedComponent { Designation = "A1", Vendor = "ОВЕН", Description = bestPlc.Name, Article = bestPlc.Article, Quantity = 1 });

                                // 2.ВЫЧИСЛЯЕМ ОСТАТОК СИГНАЛОВ
                                int remDi = Math.Max(0, config.DiCount - bestPlc.DiCount);
                                int remDo = Math.Max(0, config.DoCount - bestPlc.DoCount);
                                int remAi = Math.Max(0, config.AiCount - bestPlc.AiCount);
                                int remAo = Math.Max(0, config.AoCount - bestPlc.AoCount);

                                // 2. ИНИЦИАЛИЗИРУЕМ 4 НЕЗАВИСИМЫХ СЧЕТЧИКА ПО ГОСТУ
                                int diCounter = 1;
                                int doCounter = 1;
                                int aiCounter = 1;
                                int aoCounter = 1;

                                // 3. КВАНТОВАНИЕ СЕТЕВЫХ МОДУЛЕЙ С РАЗДЕЛЬНЫМИ СЧЕТЧИКАМИ
                                // Дискретные входы DI (Счетчик начинается с DI1)
                                if (remDi > 0)
                                {
                                    var mod = plcDb.FirstOrDefault(p => p.Article.StartsWith("МВ210") && (p.Di24Count > 0 || p.DiDryCount > 0));
                                    int capacity = mod != null ? Math.Max(mod.Di24Count, mod.DiDryCount) : 32;
                                    int count = (int)Math.Ceiling((double)remDi / capacity);

                                    for (int i = 0; i < count; i++)
                                        finalSpec.Add(new SelectedComponent { Designation = $"DI{diCounter++}", Vendor = "ОВЕН", Description = mod?.Name ?? $"Модуль дискретного ввода МВ210", Article = mod?.Article ?? "МВ210-212", Quantity = 1 });
                                }

                                // Дискретные выходы DO (Счетчик начинается с DO1)
                                if (remDo > 0)
                                {
                                    var mod = plcDb.FirstOrDefault(p => p.Article.StartsWith("МУ210") && p.DoCount > 0);
                                    int capacity = mod != null && mod.DoCount > 0 ? mod.DoCount : 16;
                                    int count = (int)Math.Ceiling((double)remDo / capacity);

                                    for (int i = 0; i < count; i++)
                                        finalSpec.Add(new SelectedComponent { Designation = $"DO{doCounter++}", Vendor = "ОВЕН", Description = mod?.Name ?? "Модуль дискретного вывода МУ210", Article = mod?.Article ?? "МУ210-402", Quantity = 1 });
                                }

                                // Аналоговые входы AI (Счетчик начинается с AI1)
                                if (remAi > 0)
                                {
                                    var mod = plcDb.FirstOrDefault(p => p.Article.StartsWith("МВ210") && p.AiCount > 0);
                                    int capacity = mod != null && mod.AiCount > 0 ? mod.AiCount : 8;
                                    int count = (int)Math.Ceiling((double)remAi / capacity);

                                    for (int i = 0; i < count; i++)
                                        finalSpec.Add(new SelectedComponent { Designation = $"AI{aiCounter++}", Vendor = "ОВЕН", Description = mod?.Name ?? "Модуль аналогового ввода МВ210", Article = mod?.Article ?? "МВ210-101", Quantity = 1 });
                                }

                                // Аналоговые выходы AO (Счетчик начинается с AO1)
                                if (remAo > 0)
                                {
                                    var mod = plcDb.FirstOrDefault(p => p.Article.StartsWith("МУ210") && p.AoCount > 0);
                                    int capacity = mod != null && mod.AoCount > 0 ? mod.AoCount : 6;
                                    int count = (int)Math.Ceiling((double)remAo / capacity);

                                    for (int i = 0; i < count; i++)
                                        finalSpec.Add(new SelectedComponent { Designation = $"AO{aoCounter++}", Vendor = "ОВЕН", Description = mod?.Name ?? "Модуль аналогового вывода МУ210", Article = mod?.Article ?? "МУ210-502", Quantity = 1 });
                                }

                            }
                        }
                        break;

                }
            }





            // ----------------------------------------------------
            // ШАГ 3 LOGIC: СИЛОВАЯ ТАБЛИЦА АВТОМАТОВ (КЭАЗ/DEKRAFT)
            // ----------------------------------------------------
            string breakersJsonPath = Path.Combine(_env.WebRootPath, "Configs", "breakers-base.json");
            if (File.Exists(breakersJsonPath))
            {
                var breakers = JsonSerializer.Deserialize<List<JsonBreakerItem>>(File.ReadAllText(breakersJsonPath));

                int contactorCounter = 1;
                foreach (var line in lines.Where(l => l.IsEnabled))
                {
                    // Ищем автомат в JSON по живым параметрам с UI строки таблицы
                    var matchedBreaker = breakers?.FirstOrDefault(b =>
                        b.Manufacturer == config.Manufacturer &&
                        b.Poles == line.Poles &&
                        b.Current == line.Current &&
                        b.Curve == line.Curve);

                    string desc = matchedBreaker != null ? matchedBreaker.Name : $"Выключатель автоматический {line.Poles}P {line.Current}A {line.Curve}";
                    string art = matchedBreaker != null ? matchedBreaker.Article : $"{config.Manufacturer}-ВА-{line.Poles}P-{line.Current}A";

                    finalSpec.Add(new SelectedComponent { Designation = line.Designation, Vendor = config.Manufacturer, Description = desc, Article = art, Quantity = 1 });

                    // Логика автоматического подбора контакторов
                    if (line.HasContactor)
                    {
                        finalSpec.Add(new SelectedComponent { Designation = $"KM{contactorCounter}", Vendor = config.Manufacturer, Description = $"Контактор модульный силовой, ток {line.Current}А, катушка ~220В", Article = $"{config.Manufacturer}-KM-{line.Current}A", Quantity = 1 });
                        contactorCounter++;
                    }
                }
            }

            // ----------------------------------------------------
            // ШАГ 4 LOGIC: КЛЕММЫ STEZ И АВТОМАТИЧЕСКИЕ СТОПОРЫ/ЗАГЛУШЕК
            // ----------------------------------------------------
            string terminalsJsonPath = Path.Combine(_env.WebRootPath, "Configs", "terminals-base.json");
            if (File.Exists(terminalsJsonPath))
            {
                var jsonTerminals = JsonSerializer.Deserialize<List<JsonTerminalItem>>(File.ReadAllText(terminalsJsonPath));

                foreach (var term in terminals)
                {
                    var matchedTerm = jsonTerminals?.FirstOrDefault(t =>
                        t.Vendor == config.TerminalVendor &&
                        t.TerminalType == term.TerminalType &&
                        t.WireSection == term.WireSection);

                    string art = matchedTerm != null ? matchedTerm.Article : $"{config.TerminalVendor}-XT";
                    string desc = matchedTerm != null ? matchedTerm.Name : $"Клемма {term.TerminalType} {term.WireSection}";

                    // Сама клемма
                    finalSpec.Add(new SelectedComponent { Designation = term.XBlockName, Vendor = config.TerminalVendor, Description = desc, Article = art, Quantity = term.Quantity });

                    // АВТОМАТИЧЕСКИЙ РАСЧЕТ ОБОЙМЫ РЯДА ПО ГОСТу! 
                    if (config.TerminalVendor == "STEZ")
                    {
                        finalSpec.Add(new SelectedComponent { Designation = $"{term.XBlockName}.Загл", Vendor = "STEZ", Description = $"Пластина торцевая изолирующая ряда {term.XBlockName}", Article = "STZ-NPP-2.5", Quantity = 1 });
                        finalSpec.Add(new SelectedComponent { Designation = $"{term.XBlockName}.Стопор", Vendor = "STEZ", Description = $"Концевой фиксатор (стопор) на DIN-рейку ряда {term.XBlockName}", Article = "STZ-KD3", Quantity = 2 });
                    }
                }
            }

            // ----------------------------------------------------
            // ШАГ 5: ПАТТЕРН 4 — ГРУППИРОВКА ОДИНАКОВЫХ АРТИКУЛОВ
            // ----------------------------------------------------
            var groupedSpec = finalSpec
                .GroupBy(c => c.Article)
                .Select(g => new SelectedComponent
                {
                    Designation = string.Join(", ", g.Select(x => x.Designation).Distinct()),
                    Vendor = g.First().Vendor,
                    Description = g.First().Description,
                    Article = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            return groupedSpec;
        }

        // Вспомогательные DTO-классы для десериализации JSON баз данных
        private class JsonCabinetItem { public string Manufacturer { get; set; } public string MountType { get; set; } public string IpRating { get; set; } public string Dimensions { get; set; } public string Article { get; set; } public string Name { get; set; } }
        private class JsonPlcItem {
            public string Manufacturer { get; set; }
            public string PlcType { get; set; }
            public string Protocol { get; set; }
            public string Article { get; set; }
            public string Name { get; set; }
            public int DiCount { get; set; }
            public int Di24Count { get; set; }  // НОВОЕ ПОЛЕ
            public int DiDryCount { get; set; } // НОВОЕ ПОЛЕ
            public int AiCount { get; set; }
            public int DoCount { get; set; }
            public int AoCount { get; set; }
            public string PowerSupply { get; set; }
            public string ModuleType { get; set; }
        }
        private class JsonBreakerItem { public string Manufacturer { get; set; } public int Poles { get; set; } public int Current { get; set; } public string Curve { get; set; } public string Article { get; set; } public string Name { get; set; } }
        private class JsonTerminalItem { public string Vendor { get; set; } public string TerminalType { get; set; } public string WireSection { get; set; } public string Article { get; set; } public string Name { get; set; } }
    }
}
