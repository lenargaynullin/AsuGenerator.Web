using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Collections.Generic;
using System.IO;

namespace AsuGenerator.Web.Services;

public class DocumentGenerator
{
    public byte[] GenerateExcelSpecification(List<SelectedComponent> components, VentAvtomatikaConfig config)
    {
        ExcelPackage.License.SetNonCommercialPersonal("AsuGeneratorSaaS");

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Спецификация оборудования");

            // --- 1. ОФОРМЛЯЕМ ШАПКУ ГОСТ ДОКУМЕНТА ---
            worksheet.Cells["A1"].Value = $"СПЕЦИФИКАЦИЯ ОБОРУДОВАНИЯ И МАТЕРИАЛОВ";
            worksheet.Cells["A1:E1"].Merge = true;
            worksheet.Cells["A1"].Style.Font.Size = 14;
            worksheet.Cells["A1"].Style.Font.Bold = true;

            worksheet.Cells["A2"].Value = $"Заказчик: {config.ClientName} ({config.CompanyName}) | КП №: {config.KpNumber}";
            worksheet.Cells["A2:E2"].Merge = true;
            worksheet.Cells["A2"].Style.Font.Italic = true;

            // --- 2. ТАБЛИЦА ЗАГОЛОВКОВ ---
            worksheet.Cells["A4"].Value = "Поз.";
            worksheet.Cells["B4"].Value = "Бренд";
            worksheet.Cells["C4"].Value = "Наименование и техническая характеристика";
            worksheet.Cells["D4"].Value = "Тип, марка, артикул";
            worksheet.Cells["E4"].Value = "Кол-во";

            using (var range = worksheet.Cells["A4:E4"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            // --- 3. ЗАПОЛНЯЕМ СТРОКИ ИЗ МАССИВА ЯДРА ---
            int row = 5;
            foreach (var comp in components)
            {
                worksheet.Cells[row, 1].Value = comp.Designation;
                worksheet.Cells[row, 2].Value = comp.Vendor;
                worksheet.Cells[row, 3].Value = comp.Description;
                worksheet.Cells[row, 4].Value = comp.Article;
                worksheet.Cells[row, 5].Value = 1; // Дефолтное кол-во

                // Сетка для таблицы
                worksheet.Cells[$"A{row}:E{row}"].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                row++;
            }

            // Автоподбор ширины столбцов под b2b-текст
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }
    }
}
