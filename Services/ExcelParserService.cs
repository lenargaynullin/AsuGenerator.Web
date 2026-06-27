using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

public partial class ExcelParserService
{
    public List<CustomTemplateItem> ParseSpecificationTable(string clipboardText)
    {
        var items = new List<CustomTemplateItem>();
        if (string.IsNullOrWhiteSpace(clipboardText)) return items;

        var lines = clipboardText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var columns = line.Split('\t');
            if (columns.Length < 3) continue; // Защита от пустых строк

            string art = columns.ElementAtOrDefault(0)?.Trim() ?? "";
            string name = columns.ElementAtOrDefault(1)?.Trim() ?? "";
            string qtyStr = columns.ElementAtOrDefault(2)?.Trim() ?? "0";
            string unit = columns.ElementAtOrDefault(3)?.Trim() ?? "шт";
            string type = columns.ElementAtOrDefault(4)?.Trim() ?? "Прочее"; // ЧИТАЕМ 5-Й СТОЛБЕЦ

            // Пропускаем шапку таблицы
            if (art.Equals("Артикул", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Номенклатура", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.IsNullOrEmpty(art) && string.IsNullOrEmpty(name)) continue;

            qtyStr = qtyStr.Replace('.', ',');
            if (!double.TryParse(qtyStr, NumberStyles.Any, new CultureInfo("ru-RU"), out double qty))
            {
                qty = 1;
            }

            items.Add(new CustomTemplateItem
            {
                PartNumber = string.IsNullOrEmpty(art) ? "—" : art,
                Name = name,
                Quantity = qty,
                Unit = unit,
                Type = string.IsNullOrEmpty(type) ? "Прочее" : type // Сохраняем тип
            });
        }

        return items;
    }
}
