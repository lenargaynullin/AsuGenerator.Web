using OfficeOpenXml;

namespace AsuGenerator.Web.Services;

public class ExcelParserService
{
    public VentAvtomatikaConfig ParseVentaFile(Stream fileStream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var config = new VentAvtomatikaConfig();

        using (var package = new ExcelPackage(fileStream))
        {
            var worksheet = package.Workbook.Worksheets[0]; // Берем первый лист

            // Чтение шапки (Заказчик)
            config.ClientName = worksheet.Cells["B3"].Value?.ToString() ?? "Не указан";
            config.CompanyName = worksheet.Cells["B4"].Value?.ToString() ?? "Не указана";
            config.KpNumber = worksheet.Cells["G5"].Value?.ToString() ?? "";

            // Нагреватель водяной (проверка крестика в ячейке A16)
            if (worksheet.Cells["A16"].Value?.ToString()?.ToUpper() == "X")
            {
                config.Heater1Type = "Водяной";
                double.TryParse(worksheet.Cells["C17"].Value?.ToString(), out var pumpPower);
                config.Heater1PumpPowerKw = pumpPower;
            }

            // Вентилятор притока (Мощность в C52)
            double.TryParse(worksheet.Cells["C52"].Value?.ToString(), out var fanPower);
            config.SupplyFanPowerKw = fanPower;

            // Тип пуска вентилятора
            if (worksheet.Cells["E54"].Value?.ToString()?.ToUpper() == "X")
                config.SupplyFanRegulation = "Частотное";
        }

        return config;
    }
}
