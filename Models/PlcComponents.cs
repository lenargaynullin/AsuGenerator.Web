using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Models
{
    public enum CabinetType
    {
        SingleSided, // Односторонний
        DoubleSided  // Двухсторонний
    }

    public class SignalRequirement
    {
        // Аналоговые входы неискробезопасные (AI-NIS)
        public int AiNisCurrent2W { get; set; }      // AI-NIS (4-20 mA, 2 w)
        public int AiNisCurrent4W { get; set; }      // AI-NIS (4-20 mA, 4 w)
        public int AiRtdNis3W { get; set; }          // AI-RTD-NIS (3 w)
        public int AiNisPl3W { get; set; }           // AI-NIS (Pl, 3 w)

        // Аналоговые входы искробезопасные (AI-IS)
        public int AiIsCurrent2W { get; set; }       // AI-IS (4-20 mA, 2 w)
        public int AiIsCurrent4W { get; set; }       // AI-IS (4-20 mA, 4 w)
        public int AiIsPl3W { get; set; }            // AI-IS (Pl, 3 w)

        // Аналоговые входы резервированные (AI-R)
        public int AiRNisCurrent2W { get; set; }     // AI-R-NIS (4-20 mA, 2 w)
        public int AiRNisCurrent4W { get; set; }     // AI-R-NIS (4-20 mA, 4 w)
        public int AiRIsCurrent2W { get; set; }      // AI-R-IS (4-20 mA, 2 w)
        public int AiRIsCurrent4W { get; set; }      // AI-R-IS (4-20 mA, 4 w)

        // Аналоговые выходы (AO)
        public int AoNisCurrent { get; set; }        // AO-NIS (4-20 mA)
        public int AoNisVoltage { get; set; }         // AO-NIS (0-10 В)
        public int AoIsVoltage { get; set; }         // AO-IS (0-10 В)
        public int AoIsCurrent { get; set; }         // AO-IS (4-20 mA)
        
        // Аналоговые выходы резервированные (AO-R)
        public int AoRIsCurrent { get; set; }        // AO-R-IS (4-20 mA)
        public int AoRNisCurrent { get; set; }       // AO-R-NIS (4-20 mA)

        // Дискретные входы (DI)
        public int DiIsNamur { get; set; }           // DI-IS (NAMUR)
        public int DiNisDryContact { get; set; }     // DI-NIS с.к.
        public int DiNis24VDC { get; set; }          // DI-NIS (24VDC)
        public int DiNisVfg { get; set; }            // DI-NIS (VFG)
        public int DiNisMcc230VAC { get; set; }      // DI-NIS (MCC 230VAC)
        public int DiNisMcc220VDC { get; set; }      // DI-NIS (MCC 220VDC)

        // Дискретные выходы (DO)
        public int DoNisDryContact { get; set; }          // DO-NIS (VFC NO)
        public int DoNis24VDC { get; set; }          // DO-NIS (24VDC)
        public int DoNisMcc230VAC { get; set; }      // DO-NIS (MCC 230VAC)
        public int DoNisMcc220VDC { get; set; }      // DO-NIS (MCC 220VDC)

        public int ReservePercent { get; set; } = 20;

        // Суммы по группам для счётчика
        public int TotalAI => AiNisCurrent2W + AiNisCurrent4W + AiRtdNis3W + AiNisPl3W
                            + AiIsCurrent2W + AiIsCurrent4W + AiIsPl3W
                            + AiRNisCurrent2W + AiRNisCurrent4W + AiRIsCurrent2W + AiRIsCurrent4W;

        public int TotalAO => AoIsVoltage + AoIsCurrent + AoNisCurrent + AoRIsCurrent + AoRNisCurrent;

        public int TotalDI => DiIsNamur + DiNisDryContact + DiNis24VDC + DiNisVfg + DiNisMcc230VAC + DiNisMcc220VDC;

        public int TotalDO => DoNisDryContact + DoNis24VDC + DoNisMcc230VAC + DoNisMcc220VDC;
    }

    public class PlcComponent
    {
        public string Article { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int DiChannels { get; set; }
        public int DoChannels { get; set; }
        public int AiChannels { get; set; }
        public int AoChannels { get; set; }
        public int WidthMm { get; set; }
        public int HeightMm { get; set; } = 125;
        public int SlotsOccupied { get; set; } = 1;


    }

    public class PlcBasket
    {
        public int BasketIndex { get; set; }
        public string BusCouplerArticle { get; set; } = string.Empty;
        public List<PlcComponent> Modules { get; set; } = new();
        public int TotalSlotsUsed => Modules.Sum(m => m.SlotsOccupied);
        public int TotalWidthMm => Modules.Sum(m => m.WidthMm);
    }

    public class PlcNode
    {
        public int NodeIndex { get; set; }
        public List<PlcBasket> Baskets { get; set; } = new();
        public int TotalNodeModules => Baskets.Sum(b => b.Modules.Count);
        public int TotalNodeWidthMm => Baskets.Sum(b => b.TotalWidthMm);
    }

    public class CabinetSide
    {
        public List<PlcNode> Nodes { get; set; } = new();
        public int TotalAllocatedHeightMm { get; set; }
        // ИСПРАВЛЕНО: Изменили с TotalDinRulesCount на TotalDinRailsCount
        public int TotalDinRailsCount { get; set; }
    }

    public class TerminalBlockRow
    {
        public int GreyTerminalsCount { get; set; }
        public int BlueTerminalsCount { get; set; }
        public int PeTerminalsCount { get; set; }
        public int EndPlatesCount { get; set; }
        public int EndStopsCount { get; set; } = 2;
        public int TotalWidthMm => (GreyTerminalsCount + BlueTerminalsCount + PeTerminalsCount) * 5 + (EndStopsCount * 8);
    }

    public class RelayUnit
    {
        public string Article { get; set; } = "48.31.7.024.0050";
        public string Name { get; set; } = "Реле промежуточное 24В 1CO";
        public int Count { get; set; }
        public int WidthMm => Count * 16;
    }

    public class PowerSupplyUnit
    {
        public string Article { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int WidthMm { get; set; }
        public int PowerWatts { get; set; }
    }

    public class PlcCabinet
    {
        public int CabinetIndex { get; set; }
        public CabinetType Type { get; set; }
        public CabinetSide FrontSide { get; set; } = new();
        public CabinetSide BackSide { get; set; } = new();
        public CabinetSide SingleSide { get; set; } = new();
        public PowerSupplyUnit? RecommendedPowerSupply { get; set; }
        public TerminalBlockRow Terminals { get; set; } = new();
        public RelayUnit InterposingRelays { get; set; } = new();
    }

    public class PlcCabinetCalculationResult
    {
        public List<PlcCabinet> Cabinets { get; set; } = new();
        // ИСПРАВЛЕНО: Здесь тоже меняем обращение к свойству на TotalDinRailsCount
        public int TotalDinRailsRequired => Cabinets.Sum(c => c.Type == CabinetType.SingleSided
            ? c.SingleSide.TotalDinRailsCount
            : (c.FrontSide.TotalDinRailsCount + c.BackSide.TotalDinRailsCount));
    }
}
