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
            // ШАГ 1 LOGIC: ПОДБОР ОБОЛОЧКИ ИЗ JSON-БАЗЫ
            // ----------------------------------------------------
            string cabinetJsonPath = Path.Combine(_env.WebRootPath, "Configs", "cabinet-base.json");
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

            // Добавляем опции Провенто (аксессуары)
            if (config.Manufacturer == "ПРОВЕНТО")
            {
                if (config.HasPocket) finalSpec.Add(new SelectedComponent { Designation = "Карман", Vendor = "ПРОВЕНТО", Description = "Карман металлический для документов А4 на дверь", Article = "PPD A4", Quantity = 1 });
                if (config.HasDoorHandle) finalSpec.Add(new SelectedComponent { Designation = "Ручка", Vendor = "ПРОВЕНТО", Description = "Эргономичная ручка двери с замком", Article = "PPH 01", Quantity = 1 });
                if (config.HasShelf) finalSpec.Add(new SelectedComponent { Designation = "Полка", Vendor = "ПРОВЕНТО", Description = "Полка внутренняя для приборов/ноутбука", Article = "PPS A4", Quantity = 1 });
                if (config.MountType == "Напольный" && config.PlinthHeight != "Нет")
                {
                    finalSpec.Add(new SelectedComponent { Designation = "Цоколь", Vendor = "ПРОВЕНТО", Description = $"Комплект разборного цоколя высотой {config.PlinthHeight}", Article = "PPL-100", Quantity = 1 });
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
