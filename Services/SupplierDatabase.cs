using System.Collections.Generic;
using System.Linq;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

public class SupplierDatabase
{
    private readonly List<BreakerEntry> _breakers = new()
    {
        new() { Article = "21231DEK", Brand = "KEAZ", Poles = 3, RatedCurrent = 18, Curve = "C", IkZ = 6, Price = 4200m },
        new() { Article = "21224DEK", Brand = "KEAZ", Poles = 3, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 3100m },
        new() { Article = "12267DEK", Brand = "KEAZ", Poles = 1, RatedCurrent = 4,  Curve = "C", IkZ = 6, Price = 380m },
        new() { Article = "12280DEK", Brand = "KEAZ", Poles = 2, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 750m },
        new() { Article = "17014DEK", Brand = "KEAZ", Poles = 4, RatedCurrent = 32, Curve = "C", IkZ = 6, Price = 1150m },
        new() { Article = "EKF-3P-18A",  Brand = "EKF", Poles = 3, RatedCurrent = 18, Curve = "C", IkZ = 6, Price = 3900m },
        new() { Article = "EKF-3P-1A",   Brand = "EKF", Poles = 3, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 2900m },
        new() { Article = "EKF-1P-4A",   Brand = "EKF", Poles = 1, RatedCurrent = 4,  Curve = "C", IkZ = 6, Price = 350m },
        new() { Article = "EKF-2P-1A",   Brand = "EKF", Poles = 2, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 700m },
        new() { Article = "EKF-4P-32A",  Brand = "EKF", Poles = 4, RatedCurrent = 32, Curve = "C", IkZ = 6, Price = 1100m },
        new() { Article = "IEK-3P-18A",  Brand = "IEK", Poles = 3, RatedCurrent = 18, Curve = "C", IkZ = 6, Price = 3600m },
        new() { Article = "IEK-3P-1A",   Brand = "IEK", Poles = 3, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 2700m },
        new() { Article = "IEK-1P-4A",   Brand = "IEK", Poles = 1, RatedCurrent = 4,  Curve = "C", IkZ = 6, Price = 320m },
        new() { Article = "IEK-2P-1A",   Brand = "IEK", Poles = 2, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 650m },
        new() { Article = "IEK-4P-32A",  Brand = "IEK", Poles = 4, RatedCurrent = 32, Curve = "C", IkZ = 6, Price = 1050m },
    };

    private readonly List<DeviceEntry> _devices = new()
    {
        // ========== КОНТАКТОРЫ ==========
        new() { Article = "18053DEK", Brand = "KEAZ", DeviceType = "Contactor", Params = new() { ["poles"] = "4", ["current"] = "16", ["voltage"] = "230" }, Price = 1850m },
        new() { Article = "18050DEK", Brand = "KEAZ", DeviceType = "Contactor", Params = new() { ["poles"] = "2", ["current"] = "16", ["voltage"] = "230" }, Price = 1400m },

        // ========== ЛАМПЫ ==========
        new() { Article = "BLS10-ADDS-230-K05", Brand = "IEK", DeviceType = "Lamp", Params = new() { ["voltage"] = "230", ["color"] = "Yellow", ["diameter"] = "22" }, Price = 190m },
        new() { Article = "BLS10-ADDS-230-K06", Brand = "IEK", DeviceType = "Lamp", Params = new() { ["voltage"] = "230", ["color"] = "Green", ["diameter"] = "22" }, Price = 190m },
        new() { Article = "BLS10-ADDS-230-K04", Brand = "IEK", DeviceType = "Lamp", Params = new() { ["voltage"] = "230", ["color"] = "Red", ["diameter"] = "22" }, Price = 190m },

        // ========== КНОПКИ ==========
        new() { Article = "BBT61-BA-K04", Brand = "IEK", DeviceType = "Button", Params = new() { ["color"] = "Red", ["type"] = "NC", ["diameter"] = "22", ["fixation"] = "Без фиксации" }, Price = 250m },
        new() { Article = "BBT50-BW-K06", Brand = "IEK", DeviceType = "Button", Params = new() { ["color"] = "Green", ["type"] = "NO", ["diameter"] = "22", ["fixation"] = "Без фиксации" }, Price = 380m },

        // ========== ПЕРЕКЛЮЧАТЕЛИ ==========
        new() { Article = "BSW60-BD-3-K02", Brand = "IEK", DeviceType = "Switch", Params = new() { ["positions"] = "3", ["diameter"] = "22", ["fixation"] = "С фиксацией" }, Price = 420m },

        // ========== РЕЛЕ ==========
        new() { Article = "MR-203D", Brand = "КИППРИБОР", DeviceType = "Relay", Params = new() { ["voltage"] = "24", ["current"] = "8", ["contacts"] = "2CO", ["indication"] = "LED" }, Price = 450m },
        new() { Article = "MR-207A", Brand = "КИППРИБОР", DeviceType = "Relay", Params = new() { ["voltage"] = "220", ["current"] = "8", ["contacts"] = "2CO", ["indication"] = "LED" }, Price = 480m },

        // ========== ПЛК ==========
        new() { Article = "ПР200-24.4.2.0", Brand = "ОВЕН", DeviceType = "PLC", Params = new() { ["voltage"] = "24", ["di"] = "8", ["do"] = "4", ["ai"] = "2", ["interface"] = "RS485" }, Price = 11200m },
        new() { Article = "ПРМ-24.1", Brand = "ОВЕН", DeviceType = "PLC", Params = new() { ["voltage"] = "24", ["di"] = "8", ["do"] = "8", ["interface"] = "Расширение" }, Price = 6200m },

        // ========== БЛОКИ ПИТАНИЯ ==========
        new() { Article = "БП60Б-Д4-24", Brand = "ОВЕН", DeviceType = "PowerSupply", Params = new() { ["inputVoltage"] = "230", ["outputVoltage"] = "24", ["current"] = "2.5", ["power"] = "60" }, Price = 3800m },

        // ========== ДАТЧИКИ ==========
        new() { Article = "ДТС3125-PT1000", Brand = "ОВЕН", DeviceType = "Sensor", Params = new() { ["type"] = "PT1000", ["signal"] = "Сопротивление", ["ip"] = "IP65" }, Price = 2500m },
        new() { Article = "ДТС3222-PT1000", Brand = "ОВЕН", DeviceType = "Sensor", Params = new() { ["type"] = "PT1000", ["signal"] = "Сопротивление", ["ip"] = "IP65" }, Price = 2800m },

        // ========== ТЕРМОСТАТЫ ==========
        new() { Article = "MTR-K3", Brand = "Meyertec", DeviceType = "Thermostat", Params = new() { ["type"] = "NC", ["current"] = "16", ["range"] = "-35..+35" }, Price = 3200m },

        // ========== КЛЕММЫ ==========
        new() { Article = "YZN30-016-K03", Brand = "IEK", DeviceType = "Terminal", Params = new() { ["section"] = "16", ["inputs"] = "2", ["type"] = "Винт" }, Price = 140m },
        new() { Article = "YZN30-002-K03", Brand = "IEK", DeviceType = "Terminal", Params = new() { ["section"] = "2.5", ["inputs"] = "2", ["type"] = "Винт" }, Price = 35m },

        // ========== ТТР ==========
        new() { Article = "ТТР-40А", Brand = "ОВЕН", DeviceType = "SSR", Params = new() { ["current"] = "40", ["voltage"] = "230", ["control"] = "4-20мА" }, Price = 4500m },
    };

    public string FindDeviceArticle(string brand, string deviceType, DeviceParamsConfig? p)
    {
        var candidates = _devices.Where(d => d.DeviceType == deviceType).ToList();

        if (p != null && candidates.Count > 1)
        {
            // Уточняем по параметрам
            if (!string.IsNullOrEmpty(p.Color))
                candidates = FilterByParam(candidates, "color", p.Color);
            if (!string.IsNullOrEmpty(p.Type))
                candidates = FilterByParam(candidates, "type", p.Type);
            if (p.Voltage > 0)
                candidates = FilterByParam(candidates, "voltage", p.Voltage.ToString());
            if (p.Section > 0)
                candidates = FilterByParam(candidates, "section", p.Section.ToString());
            if (p.Current > 0)
                candidates = FilterByParam(candidates, "current", p.Current.ToString());
        }

        var match = candidates.FirstOrDefault(d => d.Brand == brand) ?? candidates.FirstOrDefault();
        return match?.Article ?? "";
    }

    private List<DeviceEntry> FilterByParam(List<DeviceEntry> candidates, string key, string value)
    {
        var filtered = candidates.Where(d => d.Params.ContainsKey(key) && d.Params[key] == value).ToList();
        return filtered.Count > 0 ? filtered : candidates;
    }

    public string FindBreakerArticle(string brand, BreakerParams p)
    {
        var match = _breakers.FirstOrDefault(b =>
            b.Brand == brand && b.Poles == p.Poles &&
            b.RatedCurrent >= p.RatedCurrent && b.Curve == p.Curve && b.IkZ >= p.IkZ);
        if (match != null) return match.Article;

        match = _breakers.FirstOrDefault(b =>
            b.Poles == p.Poles && b.RatedCurrent >= p.RatedCurrent &&
            b.Curve == p.Curve && b.IkZ >= p.IkZ);
        return match?.Article ?? "";
    }

    public decimal FindBreakerPrice(string article)
    {
        return _breakers.FirstOrDefault(b => b.Article == article)?.Price ?? 1500m;
    }
}

public class BreakerEntry
{
    public string Article { get; set; } = "";
    public string Brand { get; set; } = "";
    public int Poles { get; set; }
    public double RatedCurrent { get; set; }
    public string Curve { get; set; } = "C";
    public int IkZ { get; set; } = 6;
    public decimal Price { get; set; }
}

public class DeviceEntry
{
    public string Article { get; set; } = "";
    public string Brand { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public Dictionary<string, string> Params { get; set; } = new();
    public decimal Price { get; set; }
}