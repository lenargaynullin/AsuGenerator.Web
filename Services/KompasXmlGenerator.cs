using System.Collections.Generic;
using System.Text;

namespace AsuGenerator.Web.Services;

public class KompasXmlGenerator
{
    /// <summary>
    /// Генерирует XML для импорта в КОМПАС-Электрик.
    /// </summary>
    public string Generate(List<DeviceDefinition> devices, List<ConnectionDefinition> connections, string projectName)
    {
        var xml = new StringBuilder();

        xml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        xml.AppendLine($"<Project Name=\"{projectName}\">");

        // === УСТРОЙСТВА ===
        xml.AppendLine("  <Devices>");
        foreach (var device in devices)
        {
            xml.AppendLine($"    <Device Id=\"{device.Designation}\" Name=\"{device.Name}\" Article=\"{device.Article}\" Manufacturer=\"{device.Manufacturer}\" Type=\"{device.Type}\">");

            // Контакты устройства
            xml.AppendLine("      <Pins>");
            foreach (var pin in device.Pins)
            {
                xml.AppendLine($"        <Pin Number=\"{pin.Number}\" Label=\"{pin.Label}\" Direction=\"{pin.Direction}\"/>");
            }
            xml.AppendLine("      </Pins>");

            xml.AppendLine("    </Device>");
        }
        xml.AppendLine("  </Devices>");

        // === СОЕДИНЕНИЯ ===
        xml.AppendLine("  <Connections>");
        foreach (var conn in connections)
        {
            xml.AppendLine($"    <Connection From=\"{conn.FromDevice}\" FromPin=\"{conn.FromPin}\" To=\"{conn.ToDevice}\" ToPin=\"{conn.ToPin}\" Wire=\"{conn.Wire}\" Color=\"{conn.Color}\" Section=\"{conn.Section}\"/>");
        }
        xml.AppendLine("  </Connections>");

        xml.AppendLine("</Project>");
        return xml.ToString();
    }
}

// === МОДЕЛИ ===

public class DeviceDefinition
{
    public string Designation { get; set; } = "";   // QS1, QF1, KM1
    public string Name { get; set; } = "";           // "Выключатель-разъединитель"
    public string Article { get; set; } = "";        // "17014DEK"
    public string Manufacturer { get; set; } = "";   // "Dekraft"
    public string Type { get; set; } = "";           // "Switch", "Breaker", "Contactor"
    public List<PinDefinition> Pins { get; set; } = new();
}

public class PinDefinition
{
    public string Number { get; set; } = "";   // "1", "2", "L1", "T1"
    public string Label { get; set; } = "";    // "Вход L1", "Выход T1"
    public string Direction { get; set; } = "InOut"; // "In", "Out", "InOut"
}

public class ConnectionDefinition
{
    public string FromDevice { get; set; } = "";  // "QS1"
    public string FromPin { get; set; } = "";      // "1"
    public string ToDevice { get; set; } = "";     // "QF1"
    public string ToPin { get; set; } = "";        // "1"
    public string Wire { get; set; } = "";         // "L1"
    public string Color { get; set; } = "";        // "Коричневый"
    public string Section { get; set; } = "";      // "2.5"
}