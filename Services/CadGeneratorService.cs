using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System.IO;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

public class CadGeneratorService
{
    private readonly DxfBlockManager _blockManager;

    public CadGeneratorService(DxfBlockManager blockManager)
    {
        _blockManager = blockManager;
    }

    public Dictionary<string, byte[]> GenerateProjectSchematics(List<SelectedComponent> components, VentAvtomatikaConfig config)
    {
        // ДИАГНОСТИКА: вывести все компоненты
        System.Diagnostics.Debug.WriteLine($"=== КОМПОНЕНТЫ ({components.Count}) ===");
        foreach (var c in components)
        {
            System.Diagnostics.Debug.WriteLine($"  {c.Designation} | {c.Article} | {c.Description}");
        }

        // ВРЕМЕННО: тестовый компонент для проверки CAD
        if (components.Count == 0)
        {
            components.Add(new SelectedComponent
            {
                Designation = "QF1",
                Article = "21231DEK",
                Description = "Автомат 3P 18A"
            });
        }

        var schematics = new Dictionary<string, byte[]>();

        // --- ЛИСТ 1: СИЛОВАЯ ЧАСТЬ (A3_1.dxf) ---
        DxfDocument sheet1 = _blockManager.GetTemplate("A3_1");
        FillStamp(sheet1, config, isFirstSheet: true);

        // Используем стандартный слой "0", который железно есть в любом DXF
        Layer defaultLayer = sheet1.Layers["0"];

        double currentX = 60;
        double currentY = 210;

        // Ищем вводной автомат QF1 из вашей b2b-спецификации
        var qf1 = components.Find(c => c.Designation == "QF1");
        if (qf1 != null)
        {
            // 1. Чертим вертикальные провода (на слое "0")
            DrawVerticalWires(sheet1, currentX, currentY + 40, 40, defaultLayer);

            // 2. Вставляем блок автомата QF_3P.dxf (на слое "0")
            InsertComponentBlock(sheet1, "QF_3P", new Vector2(currentX, currentY), qf1.Designation, qf1.Article, defaultLayer);

            // 3. Чертим отвод провода вниз
            DrawVerticalWires(sheet1, currentX, currentY - 15, 40, defaultLayer);

            // 4. Вставляем клеммник TERMINAL_2 (на слое "0")
            InsertComponentBlock(sheet1, "TERMINAL_2", new Vector2(currentX, 80), "X1", "ЗНИ-4", defaultLayer);
        }
        schematics.Add($"Схема_Лист1_КП{config.KpNumber}.dxf", SaveToBytes(sheet1));

        // --- ЛИСТ 2: АВТОМАТИКА И ПЛК (A3_2.dxf) ---
        DxfDocument sheet2 = _blockManager.GetTemplate("A3_2");
        FillStamp(sheet2, config, isFirstSheet: false);
        schematics.Add($"Схема_Лист2_КП{config.KpNumber}.dxf", SaveToBytes(sheet2));

        return schematics;
    }

    private void FillStamp(DxfDocument dxf, VentAvtomatikaConfig config, bool isFirstSheet)
    {
        Layer defaultLayer = dxf.Layers["0"];
        double stampEndX = 415;
        double stampStartY = 5;

        if (isFirstSheet)
        {
            double stampStartX = stampEndX - 185;
            dxf.Entities.Add(new netDxf.Entities.Text(config.CompanyName.ToUpper(), new Vector2(stampStartX + 5, stampStartY + 5), 5) { Layer = defaultLayer });
            dxf.Entities.Add(new netDxf.Entities.Text($"ЩУВ. КП № {config.KpNumber}", new Vector2(stampStartX + 5, stampStartY + 20), 5) { Layer = defaultLayer });
            dxf.Entities.Add(new netDxf.Entities.Text("Разработал: AsuGenerator SaaS", new Vector2(stampStartX + 5, stampStartY + 45), 3.5) { Layer = defaultLayer });
            dxf.Entities.Add(new netDxf.Entities.Text("1", new Vector2(stampEndX - 25, stampStartY + 15), 3.5) { Layer = defaultLayer });
        }
        else
        {
            double smallStampStartX = stampEndX - 110;
            dxf.Entities.Add(new netDxf.Entities.Text($"ЩУВ. КП № {config.KpNumber}", new Vector2(smallStampStartX + 5, stampStartY + 5), 4) { Layer = defaultLayer });
            dxf.Entities.Add(new netDxf.Entities.Text("2", new Vector2(stampEndX - 15, stampStartY + 5), 3.5) { Layer = defaultLayer });
        }
    }

    private void InsertComponentBlock(DxfDocument targetDxf, string blockName, Vector2 position, string designation, string article, Layer layer)
    {
        DxfDocument sourceBlock = _blockManager.GetBlock(blockName);

        // ИСПРАВЛЕНО: Используем StartPoint и EndPoint вместо Start/End для netDxf 3.x+
        foreach (var line in sourceBlock.Entities.Lines)
        {
            Vector3 startPos = line.StartPoint + new Vector3(position.X, position.Y, 0);
            Vector3 endPos = line.EndPoint + new Vector3(position.X, position.Y, 0);
            targetDxf.Entities.Add(new netDxf.Entities.Line(startPos, endPos) { Layer = layer });
        }

        foreach (var circle in sourceBlock.Entities.Circles)
        {
            Vector3 centerPos = circle.Center + new Vector3(position.X, position.Y, 0);
            targetDxf.Entities.Add(new netDxf.Entities.Circle(centerPos, circle.Radius) { Layer = layer });
        }

        foreach (var lwPolyline in sourceBlock.Entities.Polylines2D)
        {
            var clonePoly = (netDxf.Entities.Polyline2D)lwPolyline.Clone();
            foreach (var vertex in clonePoly.Vertexes)
            {
                vertex.Position = new Vector2(vertex.Position.X + position.X, vertex.Position.Y + position.Y);
            }
            clonePoly.Layer = layer;
            targetDxf.Entities.Add(clonePoly);
        }

        // Маркируем текстом на слое "0"
        netDxf.Entities.Text textDes = new netDxf.Entities.Text(designation, new Vector2(position.X - 12, position.Y - 5), 3.5) { Layer = layer };
        netDxf.Entities.Text textArt = new netDxf.Entities.Text(article, new Vector2(position.X - 12, position.Y - 22), 2.5) { Layer = layer };

        targetDxf.Entities.Add(textDes);
        targetDxf.Entities.Add(textArt);
    }

    private void DrawVerticalWires(DxfDocument dxf, double xStart, double yStart, double length, Layer layer)
    {
        double phaseStep = 10;
        for (int i = 0; i < 3; i++)
        {
            double cx = xStart + (i * phaseStep);
            dxf.Entities.Add(new netDxf.Entities.Line(new Vector2(cx, yStart), new Vector2(cx, yStart - length)) { Layer = layer });
        }
    }

    private byte[] SaveToBytes(DxfDocument dxf)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            dxf.Save(ms);
            return ms.ToArray();
        }
    }
}
