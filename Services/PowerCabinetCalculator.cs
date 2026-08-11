using System;
using System.Collections.Generic;
using System.Linq;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

/// <summary>
/// Движок логики конфигуратора силовых шкафов:
/// автоподбор артикулов из базы equipment-base.json (fallback — каталог),
/// расчёт обвязки (ПуГВ, НШВИ, DIN-модули)
/// и агрегация в спецификацию ГОСТ 21.110-2013.
/// </summary>
public class PowerCabinetCalculator
{
    private readonly PowerCabinetCatalog _catalog;
    private readonly EquipmentDatabase _equipmentDb;
    private const decimal WirePerPoleM = 0.4m;      // 0.4 м провода на полюс
    private const decimal FerrulePerPoles = 2.5m;   // наконечники = полюса × 2.5
    private const decimal ReservePercent = 1.20m;   // +20% запаса модулей

    public PowerCabinetCalculator(PowerCabinetCatalog catalog, EquipmentDatabase equipmentDb)
    {
        _catalog = catalog;
        _equipmentDb = equipmentDb;
    }

    /// <summary>Автоподбор артикула модульного автомата — сначала из базы, потом из каталога.</summary>
    public ModuleBreaker? FindModuleBreaker(int poles, int currentA, string curve)
    {
        // 1. Ищем в загруженной базе
        var match = _equipmentDb.Items
            .Where(i =>
                i.TerminalType.Equals("Автомат", StringComparison.OrdinalIgnoreCase) ||
                i.TerminalType.Contains("Автомат", StringComparison.OrdinalIgnoreCase) ||
                i.TerminalType.Contains("Выключатель", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(i =>
                (i.Name.Contains($"{poles}P", StringComparison.OrdinalIgnoreCase) ||
                 i.Name.Contains($"{poles}Р", StringComparison.OrdinalIgnoreCase)) &&
                i.Name.Contains($"{currentA}А", StringComparison.OrdinalIgnoreCase) &&
                (i.Name.Contains($"х-ка {curve}", StringComparison.OrdinalIgnoreCase) ||
                 i.Name.Contains($"{curve} ", StringComparison.OrdinalIgnoreCase)));

        if (match != null)
        {
            return new ModuleBreaker
            {
                Article = match.Article,
                Name = match.Name,
                Poles = poles,
                RatedCurrentA = currentA,
                Curve = curve,
                Manufacturer = match.Vendor,
                Price = 0
            };
        }

        // 2. Fallback: старый поиск в каталоге
        return _catalog.ModuleBreakers.FirstOrDefault(b =>
            b.Poles == poles && b.RatedCurrentA == currentA &&
            string.Equals(b.Curve, curve, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Автоподбор вводного аппарата (ВА88/ВР32).</summary>
    public PowerBreaker? FindPowerBreaker(string type, string currentStr)
    {
        if (!int.TryParse(currentStr, out var current)) return null;
        return _catalog.PowerBreakers.FirstOrDefault(p =>
            string.Equals(p.Type, type, StringComparison.OrdinalIgnoreCase) &&
            p.RatedCurrentA == current);
    }

    /// <summary>Сечение обвязки по номиналу автомата (правило ТЗ).</summary>
    public static decimal SectionForCurrent(int currentA) => currentA switch
    {
        <= 16 => 1.5m,
        <= 25 => 2.5m,
        <= 40 => 4m,
        <= 50 => 6m,
        <= 63 => 10m,
        _ => 16m
    };

    public Wire? FindWire(decimal sectionMm2) =>
        _catalog.Wires.FirstOrDefault(w => w.SectionMm2 == sectionMm2);

    public Ferrule? FindFerrule(decimal sectionMm2, bool isDouble = false) =>
        _catalog.Ferrules.FirstOrDefault(f => f.SectionMm2 == sectionMm2 && f.IsDouble == isDouble);

    private static string SectionFmt(decimal s) =>
        s == Math.Floor(s) ? ((int)s).ToString() : s.ToString("0.#");

    /// <summary>
    /// Полный расчёт сессии: корпус + ввод + автоматы + обвязка ->
    /// схлопнутые строки спецификации ГОСТ.
    /// </summary>
    public List<SpecificationLine> BuildSpecification(PowerCabinetConfig cfg)
    {
        var lines = new List<SpecificationLine>();
        var pos = 1;

        // 1. Корпус
        if (cfg.SelectedEnclosure is { } enc)
        {
            lines.Add(new SpecificationLine
            {
                Pos = pos++,
                Name = enc.Name,
                TypeMark = enc.Series,
                Article = enc.Article,
                Manufacturer = "IEK",
                Quantity = 1,
                Unit = "шт.",
                Note = $"{enc.MountType}, {enc.IpRating}, {enc.Dimensions} мм",
                UnitPrice = enc.Price,
                CollapseKey = "ENC|" + enc.Article
            });
        }

        // 2. Вводной аппарат
        if (cfg.SelectedInputBreaker is { } ib)
        {
            lines.Add(new SpecificationLine
            {
                Pos = pos++,
                Name = $"Выключатель автоматический в литом корпусе {ib.Type} {ib.Poles}P {ib.RatedCurrentA}А{(string.IsNullOrEmpty(ib.Curve) ? "" : " " + ib.Curve)}",
                TypeMark = ib.Type,
                Article = ib.Article,
                Manufacturer = "IEK",
                Quantity = 1,
                Unit = "шт.",
                Note = $"Ввод, провод ПуГВ {ib.RecommendedWireSection} мм²",
                UnitPrice = ib.Price,
                CollapseKey = "PWR|" + ib.Article
            });
        }

        // 3. Автоматы (схлопывание одинаковых по CollapseKey)
        foreach (var group in cfg.BreakerRows.Where(r => r.Quantity > 0)
                     .GroupBy(r => r.CollapseKey)
                     .OrderBy(g => g.Key))
        {
            var row = group.First();
            var breaker = FindModuleBreaker(row.Poles, row.RatedCurrentA, row.Curve);
            var totalQty = group.Sum(r => r.Quantity);
            var section = SectionForCurrent(row.RatedCurrentA);

            // Определяем, из базы или из каталога подобран артикул
            bool fromDatabase = breaker != null &&
                                _equipmentDb.Items.Any(i => i.Article == breaker.Article);

            string typeMark = fromDatabase
                ? $"IEK {breaker!.Name}"                          // Наименование из базы
                : $"ВА47-29 {row.Poles}P {row.RatedCurrentA}А {row.Curve}";  // Fallback

            string name = fromDatabase
                ? breaker!.Name                                    // Наименование из базы
                : $"Выключатель автоматический модульный {row.Poles}P {row.RatedCurrentA}А, характеристика {row.Curve}, 6 кА";

            lines.Add(new SpecificationLine
            {
                Pos = pos++,
                Name = name,
                TypeMark = typeMark,
                Article = breaker?.Article ?? "—",
                Manufacturer = breaker?.Manufacturer ?? "IEK",
                Quantity = totalQty,
                Unit = "шт.",
                Note = $"Обвязка: ПуГВ {SectionFmt(section)} мм²",
                UnitPrice = breaker?.Price ?? 0,
                CollapseKey = "BRK|" + row.CollapseKey
            });
        }

        // 4. Монтажный провод ПуГВ по сечениям (0.4 м на полюс)
        var wireBySection = new Dictionary<decimal, decimal>();
        foreach (var r in cfg.BreakerRows)
        {
            if (r.Quantity <= 0) continue;
            var s = SectionForCurrent(r.RatedCurrentA);
            var meters = WirePerPoleM * r.Poles * r.Quantity;
            wireBySection[s] = wireBySection.GetValueOrDefault(s) + meters;
        }
        foreach (var kv in wireBySection.OrderBy(k => k.Key))
        {
            var wire = FindWire(kv.Key);
            if (wire is null) continue;
            var meters = Math.Ceiling(kv.Value * 10m) / 10m;
            lines.Add(new SpecificationLine
            {
                Pos = pos++,
                Name = wire.Name,
                TypeMark = "ПуГВ",
                Article = wire.Article,
                Manufacturer = "IEK",
                Quantity = meters,
                Unit = "м",
                Note = "Монтажный провод обвязки",
                UnitPrice = wire.PricePerMeter,
                CollapseKey = "WIRE|" + SectionFmt(kv.Key)
            });
        }

        // 5. Наконечники НШВИ (полюса × 2.5, округление вверх до целого)
        var ferruleBySection = new Dictionary<decimal, int>();
        foreach (var r in cfg.BreakerRows)
        {
            if (r.Quantity <= 0) continue;
            var s = SectionForCurrent(r.RatedCurrentA);
            var count = (int)Math.Ceiling(FerrulePerPoles * r.Poles * r.Quantity);
            ferruleBySection[s] = ferruleBySection.GetValueOrDefault(s) + count;
        }
        foreach (var kv in ferruleBySection.OrderBy(k => k.Key))
        {
            var fer = FindFerrule(kv.Key, isDouble: false);
            if (fer is null) continue;
            lines.Add(new SpecificationLine
            {
                Pos = pos++,
                Name = $"Наконечник кабельный изолированный {SectionFmt(kv.Key)} мм² (НШВИ)",
                TypeMark = "НШВИ",
                Article = fer.Article,
                Manufacturer = "IEK",
                Quantity = kv.Value,
                Unit = "шт.",
                Note = "Для монтажного провода",
                UnitPrice = fer.Price,
                CollapseKey = "FER|" + SectionFmt(kv.Key)
            });
        }

        // 6. DIN-рейки и кабель-каналы (1 модуль = 18 мм, +20% запаса)
        var modules = cfg.TotalModules;
        var neededMm = modules * 18 * ReservePercent;
        var dinRail = _catalog.DinAccessories.FirstOrDefault(d =>
            string.Equals(d.Category, "Рейка", StringComparison.OrdinalIgnoreCase));
        if (dinRail is not null && dinRail.LengthM is > 0)
        {
            var railMeters = Math.Ceiling(neededMm / 1000m / dinRail.LengthM.Value * 10m) / 10m;
            lines.Add(new SpecificationLine
            {
                Pos = pos++,
                Name = dinRail.Name,
                TypeMark = "DIN",
                Article = dinRail.Article,
                Manufacturer = "IEK",
                Quantity = railMeters,
                Unit = "м",
                Note = $"{modules} модулей (+20%) × 18 мм",
                UnitPrice = dinRail.Price / dinRail.LengthM.Value,
                CollapseKey = "RAIL"
            });
        }

        var channel = _catalog.DinAccessories.FirstOrDefault(d =>
            string.Equals(d.Category, "Кабель-канал", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(d.Article, "KK-40x40-2м", StringComparison.OrdinalIgnoreCase));
        if (channel is not null && channel.LengthM is > 0)
        {
            var chanMeters = Math.Ceiling(neededMm / 1000m / channel.LengthM.Value * 10m) / 10m;
            lines.Add(new SpecificationLine
            {
                Pos = pos++,
                Name = channel.Name,
                TypeMark = "Кабель-канал",
                Article = channel.Article,
                Manufacturer = "IEK",
                Quantity = chanMeters,
                Unit = "м",
                Note = "Перфорированный, 40x40",
                UnitPrice = channel.Price / channel.LengthM.Value,
                CollapseKey = "CHANNEL"
            });
        }

        return lines;
    }

    public decimal TotalCost(PowerCabinetConfig cfg) =>
        BuildSpecification(cfg).Sum(l => l.TotalPrice);
}