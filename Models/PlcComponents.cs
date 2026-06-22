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
        public int DiDryContact { get; set; }
        public int DiWetContact { get; set; }
        public int DiFastCounter { get; set; }
        public int DoRelay { get; set; }
        public int DoTransistor { get; set; }
        public int AiCurrent2W { get; set; }
        public int AiCurrent4W { get; set; }
        public int AiVoltage { get; set; }
        public int AiRtd { get; set; }
        public int AiTc { get; set; }
        public int AoCurrent { get; set; }
        public int AoVoltage { get; set; }
        public double ReservePercent { get; set; } = 20.0;

        public int TotalDI => DiDryContact + DiWetContact + DiFastCounter;
        public int TotalDO => DoRelay + DoTransistor;
        public int TotalAI => AiCurrent2W + AiCurrent4W + AiVoltage + AiRtd + AiTc;
        public int TotalAO => AoCurrent + AoVoltage;
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
