using System;
using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Models;

// =====================================================================
//  Конфигуратор силовых шкафов (ВРУ, ЩС, ЩО)
//  Модели данных: рабочая таблица, каталог IEK, спецификация ГОСТ 21.110
// =====================================================================

/// <summary>
/// Строка рабочей таблицы быстрого ввода автоматических выключателей.
/// Пользователь вводит полюса, номинал, кривую и количество одинаковых;
/// артикул и обвязка определяются на лету сервисом.
/// </summary>
public class BreakerRow
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Количество полюсов: 1P, 2P, 3P, 4P.</summary>
    public int Poles { get; set; } = 1;

    /// <summary>Номинальный ток, А (6..63 и выше по каталогу).</summary>
    public int RatedCurrentA { get; set; } = 16;

    /// <summary>Характеристика время-токовой кривой: B, C, D.</summary>
    public string Curve { get; set; } = "C";

    /// <summary>Количество одинаковых автоматов в этой строке.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Опциональное обозначение (QF1, QF2...).</summary>
    public string? Designation { get; set; }

    /// <summary>Отображаемое имя для копирования предыдущей строки.</summary>
    public string DisplayName => $"Автомат {Poles}P {RatedCurrentA}А {Curve}";

    /// <summary>
    /// Ключ агрегации: одинаковые (Полюса, Ток, Кривая) дают один артикул IEK
    /// и должны схлопнуться в одну строку финальной спецификации.
    /// </summary>
    public string CollapseKey => $"{Poles}P|{RatedCurrentA}A|{Curve}";
}

/// <summary>
/// Корпус (оболочка): ЩМП, ВРУ, ПР11, ЩРС от IEK.
/// </summary>
public class Enclosure
{
    public string Article { get; set; } = "";
    public string Series { get; set; } = "";          // ЩМП, ВРУ, ПР11, ЩРС
    public string CabinetType { get; set; } = "";     // ВРУ, ЩС, ЩО (фильтр)
    public string IpRating { get; set; } = "";        // IP31, IP54
    public string MountType { get; set; } = "";       // Навесной / Напольный
    public string Dimensions { get; set; } = "";      // "ВхШхГ", мм
    public int HeightMm { get; set; }
    public int WidthMm { get; set; }
    public int DepthMm { get; set; }
    public int ModuleCapacity { get; set; }           // вместимость модулей
    public decimal Price { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>
/// Модульный автоматический выключатель серии ВА47-29 (IEK).
/// </summary>
public class ModuleBreaker
{
    public string Article { get; set; } = "";
    public int Poles { get; set; }
    public int RatedCurrentA { get; set; }
    public string Curve { get; set; } = "C";
    public int WidthModules { get; set; }             // = полюса
    public decimal Price { get; set; }
}

/// <summary>
/// Вводной силовой аппарат: ВА88 (литой корпус) или рубильник ВР32.
/// </summary>
public class PowerBreaker
{
    public string Article { get; set; } = "";
    public string Type { get; set; } = "";            // ВА88 / ВР32
    public int RatedCurrentA { get; set; }
    public int Poles { get; set; }
    public string? Curve { get; set; }                // опционально, напр. C
    public string RecommendedWireSection { get; set; } = "";  // вводной ПуГВ, мм²
    public int WidthModules { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Монтажный провод ПуГВ (ПВ-3) сечений 1.5..50 мм².
/// </summary>
public class Wire
{
    public string Article { get; set; } = "";
    public string Name { get; set; } = "";            // "ПуГВ 1х1.5"
    public decimal SectionMm2 { get; set; }
    public decimal PricePerMeter { get; set; }
}

/// <summary>
/// Наконечник НШВИ (одинарный) или НШВИ(2) (двойной).
/// </summary>
public class Ferrule
{
    public string Article { get; set; } = "";
    public bool IsDouble { get; set; }                // НШВИ(2)
    public decimal SectionMm2 { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// Расходные материалы на DIN-рейку: клеммы ЗНИ, DIN-рейки,
/// перфорированные кабель-каналы.
/// </summary>
public class DinAccessory
{
    public string Article { get; set; } = "";
    public string Category { get; set; } = "";        // ЗНИ / Рейка / Кабель-канал
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "шт.";
    public decimal Price { get; set; }
    public decimal? LengthM { get; set; }             // погонные метры (для реек/каналов)
}

/// <summary>
/// Строка финальной спецификации по ГОСТ 21.110-2013.
/// </summary>
public class SpecificationLine
{
    public int Pos { get; set; }
    public string Name { get; set; } = "";            // Наименование и тех. характеристика
    public string TypeMark { get; set; } = "";        // Тип, марка, обозначение документа
    public string Article { get; set; } = "";         // Код оборудования (артикул)
    public string Manufacturer { get; set; } = "IEK";
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "шт.";
    public string Note { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    /// <summary>Ключ для схлопывания одинаковых позиций.</summary>
    public string CollapseKey { get; set; } = "";
}

/// <summary>
/// Корень каталога IEK (аналог mockData), загружается из
/// wwwroot/Configs/power-cabinet.json синглтон-загрузчиком.
/// </summary>
public class PowerCabinetCatalog
{
    public List<Enclosure> Enclosures { get; set; } = [];
    public List<ModuleBreaker> ModuleBreakers { get; set; } = [];
    public List<PowerBreaker> PowerBreakers { get; set; } = [];
    public List<Wire> Wires { get; set; } = [];
    public List<Ferrule> Ferrules { get; set; } = [];
    public List<DinAccessory> DinAccessories { get; set; } = [];
    public string[] CabinetTypes { get; set; } = ["ВРУ", "ЩС", "ЩО"];
    public string[] IpRatings { get; set; } = ["IP31", "IP54"];
    public string[] InputCurrents { get; set; } = ["63", "100", "160", "250", "400", "630"];
    public string[] InputTypes { get; set; } = ["Автомат", "Рубильник"];
    public int[] ModuleNominalCurrents { get; set; } = [1, 2, 3, 4, 6, 10, 16, 20, 25, 32, 40, 50, 63];
    public string[] Curves { get; set; } = ["B", "C", "D"];
    public int[] PoleOptions { get; set; } = [1, 2, 3, 4];
}

/// <summary>
/// Корневая модель сессии проектировщика.
/// </summary>
public class PowerCabinetConfig
{
    public string CabinetType { get; set; } = "ВРУ";
    public string IpRating { get; set; } = "IP31";
    public Enclosure? SelectedEnclosure { get; set; }
    public string InputCurrent { get; set; } = "63";
    public string InputType { get; set; } = "Рубильник";
    public PowerBreaker? SelectedInputBreaker { get; set; }
    public List<BreakerRow> BreakerRows { get; set; } = [];

    public int TotalModules =>
        (SelectedInputBreaker?.WidthModules ?? 0) + BreakerRows.Sum(r => r.Poles * r.Quantity);

    public decimal? EnclosurePrice => SelectedEnclosure?.Price;
}

