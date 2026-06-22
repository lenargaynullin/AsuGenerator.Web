using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Models
{
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
        public int SlotsOccupied { get; set; } = 1;
    }

    // Добавляем этот класс сюда, чтобы Blazor его увидел через @using AsuGenerator.Models
    public class PlcBasket
    {
        public int BasketIndex { get; set; }
        public string BusCouplerArticle { get; set; } = string.Empty;
        public List<PlcComponent> Modules { get; set; } = new();
        public int TotalSlotsUsed => Modules.Sum(m => m.SlotsOccupied);
        public int TotalWidthMm => Modules.Sum(m => m.WidthMm) + (string.IsNullOrEmpty(BusCouplerArticle) ? 0 : 45);
    }
}
