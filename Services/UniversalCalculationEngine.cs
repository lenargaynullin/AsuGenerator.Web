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
                    if (cabinetWidth == 500 && cabinetWidth == 1000)
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
                    if (cabinetWidth == 600 && cabinetWidth == 1200)
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
            }

                // ----------------------------------------------------
                // ШАГ 2 LOGIC: ПОДБОР КОНТРОЛЛЕРА ОВЕН ИЗ JSON
                // ----------------------------------------------------
                string plcJsonPath = Path.Combine(_env.WebRootPath, "Configs", "plc-base.json");
            if (File.Exists(plcJsonPath))
            {
                var plcs = JsonSerializer.Deserialize<List<JsonPlcItem>>(File.ReadAllText(plcJsonPath));
                var matchedPlc = plcs?.FirstOrDefault(p => p.PlcType == config.PlcType && p.Protocol == config.Protocol);

                if (matchedPlc != null)
                {
                    finalSpec.Add(new SelectedComponent { Designation = "DD1", Vendor = "ОВЕН", Description = matchedPlc.Name, Article = matchedPlc.Article, Quantity = 1 });
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
        private class JsonPlcItem { public string PlcType { get; set; } public string Protocol { get; set; } public string Article { get; set; } public string Name { get; set; } }
        private class JsonBreakerItem { public string Manufacturer { get; set; } public int Poles { get; set; } public int Current { get; set; } public string Curve { get; set; } public string Article { get; set; } public string Name { get; set; } }
        private class JsonTerminalItem { public string Vendor { get; set; } public string TerminalType { get; set; } public string WireSection { get; set; } public string Article { get; set; } public string Name { get; set; } }
    }
}
