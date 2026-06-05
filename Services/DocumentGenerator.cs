using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

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
                worksheet.Cells[row, 5].Value = comp.Quantity; // Дефолтное кол-во

                // Сетка для таблицы
                worksheet.Cells[$"A{row}:E{row}"].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
                row++;
            }

            // Автоподбор ширины столбцов под b2b-текст
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }
    }

    public byte[] GenerateTkpPdf(CommercialProposal proposal)
    {
        // Явное указание пространства имён для глобальных настроек лицензии
        QuestPDF.Settings.License = LicenseType.Community;

        // Иерархия стилей с использованием актуального API QuestPDF (.FontColor)
        var titleStyle = TextStyle.Default.FontFamily(Fonts.Arial).FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
        var subtitleStyle = TextStyle.Default.FontFamily(Fonts.Arial).FontSize(12).Bold().FontColor(Colors.Black);
        var normalStyle = TextStyle.Default.FontFamily(Fonts.Arial).FontSize(10).FontColor(Colors.Black);
        var italicStyle = TextStyle.Default.FontFamily(Fonts.Arial).FontSize(10).Italic().FontColor(Colors.Grey.Darken3);
        var footerStyle = TextStyle.Default.FontFamily(Fonts.Arial).FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
        var totalStyle = TextStyle.Default.FontFamily(Fonts.Arial).FontSize(14).Bold().FontColor(Colors.Green.Darken3);


        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(normalStyle);

                // Глобальный Header оставляем пустым, чтобы шапка не дублировалась на странице 2
                page.Header().Height(0);

                // Содержимое
                page.Content().PaddingVertical(0.5f, Unit.Centimetre).Column(col =>
                {
                    // --- ШАПКА ДОКУМЕНТА (Внутри контента, напечатается ровно 1 раз) ---
                    col.Item().Row(row =>
                    {
                        // Левая часть: Название и описание проекта
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("ТЕХНИКО-КОММЕРЧЕСКОЕ ПРЕДЛОЖЕНИЕ")
                                .Style(titleStyle);

                            column.Item().Text($"На поставку шкафа автоматизации: {proposal.ProjectName}")
                                .Style(italicStyle);
                        });

                        // Правая часть: Увеличенная колонка даты без переносов
                        row.ConstantItem(130).AlignRight().Column(column =>
                        {
                            column.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy}")
                                .Style(normalStyle).Bold();
                        });
                    });

                    // Разделительная линия под шапкой
                    col.Item().PaddingTop(10).PaddingBottom(15).Background(Colors.Grey.Lighten2).Height(1);

                    // --- 1. ТЕХНИЧЕСКОЕ ОПИСАНИЕ СИСТЕМЫ ---
                    col.Item().PaddingBottom(5).Text("1. Техническое описание системы")
                        .Style(subtitleStyle);

                    col.Item().PaddingBottom(15).Text($"Предлагаемый шкаф управления спроектирован для автоматизации приточно-вытяжной вентиляции заказчика ({proposal.ClientName}).")
                        .Style(normalStyle);

                    // --- 2. СПЕЦИФИКАЦИЯ ОБОРУДОВАНИЯ И СТОИМОСТЬ ---
                    col.Item().PaddingBottom(5).Text("2. Спецификация оборудования и стоимость")
                        .Style(subtitleStyle);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30); // №
                            columns.RelativeColumn();   // Наименование
                            columns.ConstantColumn(40); // Кол-во
                            columns.ConstantColumn(80); // Цена, руб
                            columns.ConstantColumn(80); // Сумма, руб
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("№").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Наименование").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Кол-во").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Цена").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Сумма").Bold();
                        });

                        int index = 1;
                        foreach (var item in proposal.Items)
                        {
                            table.Cell().Padding(5).Text(index.ToString()).Style(normalStyle);
                            table.Cell().Padding(5).Text(item.Name).Style(normalStyle);
                            table.Cell().Padding(5).Text(item.Quantity.ToString()).Style(normalStyle);
                            table.Cell().Padding(5).Text($"{item.FinalUnitPrice:N2}").Style(normalStyle);
                            table.Cell().Padding(5).Text($"{item.FinalRowPrice:N2}").Style(normalStyle);
                            index++;
                        }

                        // Строка сборки
                        table.Cell().Padding(5).Text(index.ToString()).Style(normalStyle);
                        table.Cell().Padding(5).Text("Комплект монтажных материалов, сборка шкафа и тестирование").Style(normalStyle);
                        table.Cell().Padding(5).Text("1").Style(normalStyle);
                        table.Cell().Padding(5).Text($"{proposal.AssemblyPrice:N2}").Style(normalStyle);
                        table.Cell().Padding(5).Text($"{proposal.AssemblyPrice:N2}").Style(normalStyle);
                    });

                    // Итоговая сумма
                    col.Item().AlignRight().PaddingTop(15).Text($"ИТОГО: {proposal.TotalPrice:N2} руб.")
                        .Style(totalStyle);
                });

                // Подвал (Печатается внизу каждого листа)
                page.Footer().AlignCenter().Column(col =>
                {
                    col.Item().Background(Colors.Grey.Lighten2).Height(1);

                    col.Item().PaddingTop(5).Text("В стоимость включена исполнительная документация по ГОСТ в формате DXF/PDF.")
                        .Style(footerStyle);
                });
            });
        }).GeneratePdf();
    }

}
