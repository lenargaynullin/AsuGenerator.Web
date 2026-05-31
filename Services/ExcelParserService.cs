using OfficeOpenXml;
using System;
using System.IO;

namespace AsuGenerator.Web.Services;

public class ExcelParserService
{
    public VentAvtomatikaConfig ParseVentaFile(Stream fileStream)
    {
        // Настройка Community лицензии для EPPlus 8+
        ExcelPackage.License.SetNonCommercialPersonal("AsuGeneratorSaaS");

        var config = new VentAvtomatikaConfig();

        using (var package = new ExcelPackage(fileStream))
        {
            var workbook = package.Workbook;

            // Хелпер для быстрого и безопасного чтения строк из именованных диапазонов
            string ReadValue(string name) => workbook.Names.ContainsKey(name) ? workbook.Names[name].Value?.ToString() ?? "" : "";

            // Хелпер для проверки флагов (Да / X / True)
            bool ReadBool(string name)
            {
                var val = ReadValue(name).ToUpper().Trim();
                return val == "X" || val == "ДА" || val == "TRUE" || val == "1";
            }

            // Хелпер для чтения числовых b2b-параметров (кВт, А)
            double ReadDouble(string name)
            {
                double.TryParse(ReadValue(name), out var res);
                return res;
            }

            // --- 1. ПАРСИНГ ОБЩИХ ДАННЫХ ---
            config.ProjectNumber = ReadValue("PROJECT_NUMBER");
            config.CabinetName = ReadValue("CABINET_NAME");
            config.DocDesignation = ReadValue("DOC_DESIGNATION");
            config.CompanyName = ReadValue("COMPANY_NAME");
            config.ClientName = ReadValue("CLIENT_NAME");
            config.KpNumber = ReadValue("KP_NUMBER");

            // --- 2. ПАРСИНГ ВОЗДУШНОГО КЛАПАНА ПРИТОКА (Исправлено Valve) ---
            config.ValveInVoltage = ReadValue("VALVE_IN_VOLTAGE");
            config.ValveInSpring = ReadBool("VALVE_IN_SPRING");

            // --- 3. ПАРСИНГ НАГРЕВАТЕЛЕЙ (Исправлено HeaterEl1) ---
            config.HeaterEl1Power = ReadDouble("HEATER_EL1_POWER");
            config.HeaterEl1Voltage = ReadValue("HEATER_EL1_VOLTAGE");

            // Если в ТЗ указана мощность ТЭНов, софт автоматически активирует электронагрев
            if (config.HeaterEl1Power > 0)
            {
                config.Heater1Type = "Электрический";
            }
            else
            {
                config.Heater1Type = "Водяной";
                config.HeaterW1PumpPower = ReadDouble("HEATER_W1_PUMP_POWER");
            }

            // --- 4. ПАРСИНГ ПРИТОЧНОГО ВЕНТИЛЯТОРА ---
            config.SupplyFanPowerKw = ReadDouble("FAN_IN_POWER");
            config.SupplyFanRegulation = ReadValue("FAN_IN_REGULATION");
            config.FanInReserve = ReadBool("FAN_IN_RESERVE");

            // --- 5. ПАРСИНГ ДОПОЛНИТЕЛЬНЫХ ОПЦИЙ ---
            config.BreakerBrand = ReadValue("BREAKER_BRAND");
            config.EnclosureType = ReadValue("ENCLOSURE_TYPE");
        }

        return config;
    }
}
