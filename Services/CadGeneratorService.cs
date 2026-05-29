using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using System.IO;

namespace AsuGenerator.Web.Services;

public class CadGeneratorService
{
    public byte[] GenerateA3Schematic(VentAvtomatikaConfig config)
    {
        // 1. Создаем новый DXF документ
        DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2007);

        // 2. Создаем b2b-слои с корректными ГОСТ-цветами
        // В netDxf белый цвет по ГОСТу — это индекс 7 (White/Black при печати)
        Layer layerBorder = new Layer("0_Рамка") { Color = new AciColor(7) };
        Layer layerFormat = new Layer("0_Текст_Штамп") { Color = AciColor.Cyan };
        Layer layerWires = new Layer("1_Провода_Сила") { Color = AciColor.Red };

        dxf.Layers.Add(layerBorder);
        dxf.Layers.Add(layerFormat);
        dxf.Layers.Add(layerWires);

        double width = 420;
        double height = 297;

        // 3. ЧЕРТИМ ВНЕШНЮЮ ГРАНИЦУ ЛИСТА
        AddRectangle(dxf, 0, 0, width, height, layerFormat);

        // 4. ЧЕРТИМ ВНУТРЕННЮЮ ГОСТ-РАМКУ (отступы 20, 5, 5, 5)
        double xMin = 20;
        double xMax = width - 5;
        double yMin = 5;
        double yMax = height - 5;

        AddRectangle(dxf, xMin, yMin, xMax - xMin, yMax - yMin, layerBorder);

        // 5. ЧЕРТИМ ШТАМП ГОСТ (185 х 55 мм)
        double stampWidth = 185;
        double stampHeight = 55;
        double sX = xMax - stampWidth;
        double sY = yMin;

        AddRectangle(dxf, sX, sY, stampWidth, stampHeight, layerBorder);

        // Чертим разделительную линию внутри штампа
        Line divisionLine = new Line(new Vector2(sX, sY + 15), new Vector2(xMax, sY + 15)) { Layer = layerBorder };
        dxf.Entities.Add(divisionLine);

        // 6. НАПОЛНЯЕМ ШТАМП ТЕКСТОМ
        Text textCompany = new Text(config.CompanyName.ToUpper(), new Vector2(sX + 5, sY + 5), 5)
        {
            Layer = layerFormat,
            Color = AciColor.Yellow
        };
        dxf.Entities.Add(textCompany);

        string docTitle = $"ЩУВ. КП № {config.KpNumber}";
        Text textTitle = new Text(docTitle, new Vector2(sX + 5, sY + 20), 6)
        {
            Layer = layerFormat,
            Style = TextStyle.Default // Исправлено Font_Style -> Style
        };
        dxf.Entities.Add(textTitle);

        Text textDeveloper = new Text("Разработал: AsuGenerator SaaS", new Vector2(sX + 5, sY + 45), 3.5)
        {
            Layer = layerFormat
        };
        dxf.Entities.Add(textDeveloper);

        // 7. СОХРАНЯЕМ В МАССИВ БАЙТОВ
        using (MemoryStream ms = new MemoryStream())
        {
            dxf.Save(ms);
            return ms.ToArray();
        }
    }

    private void AddRectangle(DxfDocument dxf, double x, double y, double w, double height, Layer layer)
    {
        Vector2 p1 = new Vector2(x, y);
        Vector2 p2 = new Vector2(x + w, y);
        Vector2 p3 = new Vector2(x + w, y + height);
        Vector2 p4 = new Vector2(x, y + height);

        dxf.Entities.Add(new Line(p1, p2) { Layer = layer });
        dxf.Entities.Add(new Line(p2, p3) { Layer = layer });
        dxf.Entities.Add(new Line(p3, p4) { Layer = layer });
        dxf.Entities.Add(new Line(p4, p1) { Layer = layer });
    }
}
