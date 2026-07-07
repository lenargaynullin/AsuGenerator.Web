using AsuGenerator.Web.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;

namespace AsuGenerator.Web.Services;

public class DocumentGenerator
{
    public byte[] GenerateExcelSpecification(List<SelectedComponent> components, VentAvtomatikaConfig config)
    {
        // Установка некоммерческой лицензии EPPlus
        ExcelPackage.License.SetNonCommercialPersonal("AsuGeneratorSaaS");

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Спецификация");

            // Включаем отображение сетки таблицы по ГОСТ
            worksheet.View.ShowGridLines = true;

            // --- 1. ОФОРМЛЕНИЕ ШАПКИ СТРАНИЦЫ ---
            worksheet.Cells["A1"].Value = "СПЕЦИФИКАЦИЯ ОБОРУДОВАНИЯ, ИЗДЕЛИЙ И МАТЕРИАЛОВ";
            worksheet.Cells["A1:I1"].Merge = true;
            worksheet.Cells["A1"].Style.Font.Size = 12;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A2"].Value = $"Шифр проекта: {config.KpNumber} | Заказчик: {config.ClientName} ({config.CompanyName})";
            worksheet.Cells["A2:I2"].Merge = true;
            worksheet.Cells["A2"].Style.Font.Italic = true;

            // --- 2. СОЗДАНИЕ СТРОГИХ ЗАГОЛОВКОВ ГРАФ ПО ГОСТ 21.110-2013 ---
            string[] headers = {
                "Поз.",
                "Наименование и техническая характеристика",
                "Тип, марка, артикул",
                "Код оборуд.",
                "Завод-изг.",
                "Ед.изм.",
                "Кол-во",
                "Масса, кг",
                "Примечание"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[4, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 10;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Границы для шапки таблицы
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }
            worksheet.Row(4).Height = 28; // Задаем высоту для шапки

            // --- 3. ЦИКЛ ЗАПОЛНЕНИЯ СТРОК ИЗ UI-ТАБЛИЦ ---
            int row = 5;
            int index = 1;
            foreach (var comp in components)
            {
                worksheet.Cells[row, 1].Value = comp.Designation; // Поз. ОУ (QF1, XT1)
                worksheet.Cells[row, 2].Value = comp.Description; // Техническое описание
                worksheet.Cells[row, 3].Value = comp.Article;     // Артикул (Провенто/КЭАЗ)
                worksheet.Cells[row, 4].Value = "-";              // Код оборудования
                worksheet.Cells[row, 5].Value = comp.Vendor;      // Производитель
                worksheet.Cells[row, 6].Value = "шт.";            // Ед. изм.
                worksheet.Cells[row, 7].Value = comp.Quantity;    // Количество
                worksheet.Cells[row, 8].Value = "";               // Масса единицы
                worksheet.Cells[row, 9].Value = "";               // Примечание

                // Накладываем тонкую сетку на каждую ячейку строки по ГОСТ
                for (int col = 1; col <= 9; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    cell.Style.Font.Size = 10;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Центрируем короткие технические данные
                    if (col == 1 || col == 3 || col == 6 || col == 7)
                    {
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                }
                worksheet.Row(row).Height = 20;
                row++;
            }

            // --- 4. ЖЕСТКАЯ КАЛИБРОВКА ШИРИНЫ КОЛОНОК ПОД СТАНДАРТЫ СПДС ---
            worksheet.Column(1).Width = 10;  // Поз.
            worksheet.Column(2).Width = 45;  // Наименование (самая широкая графа)
            worksheet.Column(3).Width = 25;  // Артикул
            worksheet.Column(4).Width = 12;  // Код
            worksheet.Column(5).Width = 15;  // Завод
            worksheet.Column(6).Width = 8;   // Ед. изм.
            worksheet.Column(7).Width = 10;  // Кол-во
            worksheet.Column(8).Width = 10;  // Масса
            worksheet.Column(9).Width = 15;  // Примечание

            return package.GetAsByteArray();
        }
    }
    /// <summary>
    /// Генерирует XLSX-файл сравнения ПЛК для нескольких вендоров.
    /// </summary>
    public byte[] GeneratePlcComparisonExcel(
        Dictionary<string, PlcComparisonResult> tkpReport,
        List<(string SignalType, int Count)> inputSignals,
        double cabinetWidthMm)
    {
        ExcelPackage.License.SetNonCommercialPersonal("AsuGeneratorSaaS");

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("TKP Сравнение ПЛК");

        ws.View.ShowGridLines = true;

        // 1. Шапка
        ws.Cells["A1"].Value = "ТЕХНИКО-КОММЕРЧЕСКОЕ ПРЕДЛОЖЕНИЕ: СРАВНЕНИЕ СИСТЕМ ПЛК";
        ws.Cells["A1:G1"].Merge = true;
        ws.Cells["A1"].Style.Font.Size = 14;
        ws.Cells["A1"].Style.Font.Bold = true;
        ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        ws.Cells["A2"].Value = $"Дата: {DateTime.Now:dd.MM.yyyy} | Ширина шкафа: {cabinetWidthMm} мм";
        ws.Cells["A2:G2"].Merge = true;
        ws.Cells["A2"].Style.Font.Size = 10;

        // 2. Входные сигналы
        int row = 4;
        ws.Cells[row, 1].Value = "ВХОДНЫЕ СИГНАЛЫ:";
        ws.Cells[row, 1].Style.Font.Bold = true;
        row++;
        foreach (var sig in inputSignals)
        {
            ws.Cells[row, 1].Value = sig.SignalType;
            ws.Cells[row, 2].Value = sig.Count;
            ws.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            row++;
        }

        // 3. Таблица сравнения
        row++;
        ws.Cells[row, 1].Value = "ПАРАМЕТР";
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

        int col = 2;
        var vendorKeys = tkpReport.Keys.ToList();
        foreach (var key in vendorKeys)
        {
            ws.Cells[row, col].Value = tkpReport[key].VendorName;
            ws.Cells[row, col].Style.Font.Bold = true;
            ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            ws.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            col++;
        }
        row++;

        // Строки параметров
        var paramRows = new List<(string Param, Func<PlcComparisonResult, string> Getter)>
        {
            ("Серия контроллера", r => r.SeriesId),
            ("Назначение", r => r.TargetApplication),
            ("Количество крейтов", r => $"{r.TotalRacksCount} шт."),
            ("Необходимо шкафов", r => $"{r.TotalCabinetsCount} шт."),
            ("Срок поставки", r => r.DeliveryTime),
            ("Стоимость оборудования, ₽", r => r.TotalHardwareCostRub.ToString("N0")),
        };

        foreach (var (param, getter) in paramRows)
        {
            ws.Cells[row, 1].Value = param;
            ws.Cells[row, 1].Style.Font.Bold = param.Contains("Стоимость");
            col = 2;
            foreach (var key in vendorKeys)
            {
                ws.Cells[row, col].Value = getter(tkpReport[key]);
                ws.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (param.Contains("Стоимость"))
                {
                    ws.Cells[row, col].Style.Font.Bold = true;
                    ws.Cells[row, col].Style.Font.Size = 14;
                }
                col++;
            }
            row++;
        }

        // 4. Ширина колонок
        ws.Column(1).Width = 30;
        for (int i = 2; i <= vendorKeys.Count + 1; i++)
            ws.Column(i).Width = 35;

        return package.GetAsByteArray();
    }
}
