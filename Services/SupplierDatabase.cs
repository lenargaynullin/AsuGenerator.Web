using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Services;

public class SupplierDatabase
{
    private readonly List<DeviceEntry> _devices =
[
    new() { Article = "21231DEK", Brand = "KEAZ", DeviceType = "Breaker", Params = new() { ["poles"] = "3", ["current"] = "18", ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "21224DEK", Brand = "KEAZ", DeviceType = "Breaker", Params = new() { ["poles"] = "3", ["current"] = "1",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "12267DEK", Brand = "KEAZ", DeviceType = "Breaker", Params = new() { ["poles"] = "1", ["current"] = "4",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "12280DEK", Brand = "KEAZ", DeviceType = "Breaker", Params = new() { ["poles"] = "2", ["current"] = "1",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "17014DEK", Brand = "KEAZ", DeviceType = "Breaker", Params = new() { ["poles"] = "4", ["current"] = "32", ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "EKF-3P-18A",  Brand = "EKF", DeviceType = "Breaker", Params = new() { ["poles"] = "3", ["current"] = "18", ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "EKF-3P-1A",   Brand = "EKF", DeviceType = "Breaker", Params = new() { ["poles"] = "3", ["current"] = "1",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "EKF-1P-4A",   Brand = "EKF", DeviceType = "Breaker", Params = new() { ["poles"] = "1", ["current"] = "4",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "EKF-2P-1A",   Brand = "EKF", DeviceType = "Breaker", Params = new() { ["poles"] = "2", ["current"] = "1",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "EKF-4P-32A",  Brand = "EKF", DeviceType = "Breaker", Params = new() { ["poles"] = "4", ["current"] = "32", ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "IEK-3P-18A",  Brand = "IEK", DeviceType = "Breaker", Params = new() { ["poles"] = "3", ["current"] = "18", ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "IEK-3P-1A",   Brand = "IEK", DeviceType = "Breaker", Params = new() { ["poles"] = "3", ["current"] = "1",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "IEK-1P-4A",   Brand = "IEK", DeviceType = "Breaker", Params = new() { ["poles"] = "1", ["current"] = "4",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "IEK-2P-1A",   Brand = "IEK", DeviceType = "Breaker", Params = new() { ["poles"] = "2", ["current"] = "1",  ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "IEK-4P-32A",  Brand = "IEK", DeviceType = "Breaker", Params = new() { ["poles"] = "4", ["current"] = "32", ["curve"] = "C", ["ikz"] = "6" } },
    new() { Article = "18053DEK", Brand = "KEAZ", DeviceType = "Contactor", Params = new() { ["poles"] = "4", ["current"] = "16", ["voltage"] = "230" } },
    new() { Article = "18050DEK", Brand = "KEAZ", DeviceType = "Contactor", Params = new() { ["poles"] = "2", ["current"] = "16", ["voltage"] = "230" } },
    new() { Article = "BLS10-ADDS-230-K05", Brand = "IEK", DeviceType = "Lamp", Params = new() { ["voltage"] = "230", ["color"] = "Yellow", ["diameter"] = "22" } },
    new() { Article = "BLS10-ADDS-230-K06", Brand = "IEK", DeviceType = "Lamp", Params = new() { ["voltage"] = "230", ["color"] = "Green", ["diameter"] = "22" } },
    new() { Article = "BLS10-ADDS-230-K04", Brand = "IEK", DeviceType = "Lamp", Params = new() { ["voltage"] = "230", ["color"] = "Red", ["diameter"] = "22" } },
    new() { Article = "BBT61-BA-K04", Brand = "IEK", DeviceType = "Button", Params = new() { ["color"] = "Red", ["type"] = "NC", ["diameter"] = "22" } },
    new() { Article = "BBT50-BW-K06", Brand = "IEK", DeviceType = "Button", Params = new() { ["color"] = "Green", ["type"] = "NO", ["diameter"] = "22" } },
    new() { Article = "BSW60-BD-3-K02", Brand = "IEK", DeviceType = "Switch", Params = new() { ["positions"] = "3", ["diameter"] = "22" } },
    new() { Article = "MR-203D", Brand = "КИППРИБОР", DeviceType = "Relay", Params = new() { ["voltage"] = "24", ["current"] = "8", ["contacts"] = "2CO" } },
    new() { Article = "MR-207A", Brand = "КИППРИБОР", DeviceType = "Relay", Params = new() { ["voltage"] = "220", ["current"] = "8", ["contacts"] = "2CO" } },
    new() { Article = "ПР200-24.4.2.0", Brand = "ОВЕН", DeviceType = "PLC", Params = new() { ["voltage"] = "24", ["di"] = "8", ["do"] = "4", ["ai"] = "2", ["interface"] = "RS485" } },
    new() { Article = "ПРМ-24.1", Brand = "ОВЕН", DeviceType = "PLC", Params = new() { ["voltage"] = "24", ["di"] = "8", ["do"] = "8", ["interface"] = "Расширение" } },
    new() { Article = "БП60Б-Д4-24", Brand = "ОВЕН", DeviceType = "PowerSupply", Params = new() { ["inputVoltage"] = "230", ["outputVoltage"] = "24", ["current"] = "2.5", ["power"] = "60" } },
    new() { Article = "ДТС3125-PT1000", Brand = "ОВЕН", DeviceType = "Sensor", Params = new() { ["type"] = "PT1000", ["ip"] = "IP65" } },
    new() { Article = "ДТС3222-PT1000", Brand = "ОВЕН", DeviceType = "Sensor", Params = new() { ["type"] = "PT1000", ["ip"] = "IP65" } },
    new() { Article = "MTR-K3", Brand = "Meyertec", DeviceType = "Thermostat", Params = new() { ["type"] = "NC", ["current"] = "16" } },
    new() { Article = "YZN30-016-K03", Brand = "IEK", DeviceType = "Terminal", Params = new() { ["section"] = "16", ["inputs"] = "2" } },
    new() { Article = "YZN30-002-K03", Brand = "IEK", DeviceType = "Terminal", Params = new() { ["section"] = "2.5", ["inputs"] = "2" } },
    new() { Article = "ТТР-40А", Brand = "ОВЕН", DeviceType = "SSR", Params = new() { ["current"] = "40", ["voltage"] = "230" } },
];

    public string FindArticle(string brand, string deviceType, Dictionary<string, string>? requiredParams)
    {
        var candidates = _devices.Where(d => d.DeviceType == deviceType).ToList();

        if (requiredParams != null && candidates.Count > 1)
        {
            foreach (var param in requiredParams)
            {
                var filtered = candidates.Where(d => d.Params.ContainsKey(param.Key) && d.Params[param.Key] == param.Value).ToList();
                if (filtered.Count > 0)
                    candidates = filtered;
            }
        }

        return candidates.FirstOrDefault(d => d.Brand == brand)?.Article
            ?? candidates.FirstOrDefault()?.Article
            ?? "";
    }
}

public class DeviceEntry
{
    public string Article { get; set; } = "";
    public string Brand { get; set; } = "";
    public string DeviceType { get; set; } = "";
    public Dictionary<string, string> Params { get; set; } = [];
}