using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Services;

public class CadGeneratorService
{
    private readonly DxfBlockManager _blockManager;

    public CadGeneratorService(DxfBlockManager blockManager)
    {
        _blockManager = blockManager;
    }

    // Главный метод генерации чертежей проекта
    public Dictionary<string, byte[]> GenerateProjectSchematics(List<SelectedComponent> components, VentAvtomatikaConfig config)
    {
        var schematics = new Dictionary<string, byte[]>();

        // --- ЛИСТ 1: СИЛОВАЯ ЧАСТЬ (A3_1.dxf) ---
        DxfDocument sheet1 = _blockManager.GetTemplate("A3_1");
        FillStamp(sheet1, config, isFirstSheet: true);

        double currentX = 60;
        double currentY = 210;

        var qf2 = components.Find(c => c.Designation == "QF2");
        if (qf2 != null)
        {
            Layer layerWires = sheet1.Layers.Contains("1_Провода_Сила") ? sheet1.Layers["1_Провода_Сила"] : new Layer("1_Провода_Сила");
            Layer layerFormat = sheet1.Layers.Contains("0_Текст_Штамп") ? sheet1.Layers["0_Текст_Штамп"] : new Layer("0_Текст_Штамп");

            DrawVerticalWires(sheet1, currentX, currentY + 40, 40, layerWires);
            InsertComponentBlock(sheet1, "QF_3P", new Vector2(currentX, currentY), qf2.Designation, qf2.Article, layerWires, layerFormat);
            DrawVerticalWires(sheet1, currentX, currentY - 15, 40, layerWires);
        }
        schematics.Add($"Схема_Лист1_Сила_КП{config.KpNumber}.dxf", SaveToBytes(sheet1));

        // --- ЛИСТ 2: АВТОМАТИКА И ПЛК (A3_2.dxf) ---
        DxfDocument sheet2 = _blockManager.GetTemplate("A3_2");
        FillStamp(sheet2, config, isFirstSheet: false);
        schematics.Add($"Схема_Лист2_Автоматика_КП{config.KpNumber}.dxf", SaveToBytes(sheet2));

        return schematics;
    }

    // Умный b2b-метод автоматического заполнения штампов по ГОСТу
    private void FillStamp(DxfDocument dxf, VentAvtomatikaConfig config, bool isFirstSheet)
    {
        Layer layerFormat = dxf.Layers.Contains("0_Текст_Штамп")
            ? dxf.Layers["0_Текст_Штамп"]
            : new Layer("0_Текст_Штамп");

        double stampEndX = 415;
        double stampStartY = 5;

        if (isFirstSheet)
        {
            double stampStartX = stampEndX - 185;
            dxf.Entities.Add(new netDxf.Entities.Text(config.CompanyName.ToUpper(),
                new Vector2(stampStartX + 5, stampStartY + 5), 5)
            { Layer = layerFormat });
            dxf.Entities.Add(new netDxf.Entities.Text($"ЩУВ. КП № {config.KpNumber}",
                new Vector2(stampStartX + 5, stampStartY + 20), 5)
            { Layer = layerFormat });
            dxf.Entities.Add(new netDxf.Entities.Text("Разработал: AsuGenerator SaaS",
                new Vector2(stampStartX + 5, stampStartY + 45), 3.5)
            { Layer = layerFormat });
            dxf.Entities.Add(new netDxf.Entities.Text("1",
                new Vector2(stampEndX - 25, stampStartY + 15), 3.5)
            { Layer = layerFormat });
        }
        else
        {
            double smallStampStartX = stampEndX - 110;
            dxf.Entities.Add(new netDxf.Entities.Text($"ЩУВ. КП № {config.KpNumber}",
                new Vector2(smallStampStartX + 5, stampStartY + 5), 4)
            { Layer = layerFormat });
            dxf.Entities.Add(new netDxf.Entities.Text("2",
                new Vector2(stampEndX - 15, stampStartY + 5), 3.5)
            { Layer = layerFormat });
        }
    }

    // Универсальный метод вставки сложного b2b УГО-блока из файла .dxf
    private void InsertComponentBlock(DxfDocument targetDxf, string blockName, Vector2 position,
    string designation, string article, Layer layerWires, Layer layerFormat)
    {
        DxfDocument sourceBlock = _blockManager.GetBlock(blockName);

        foreach (var line in sourceBlock.Entities.Lines)
        {
            Vector3 start = new Vector3(
                line.StartPoint.X + position.X,
                line.StartPoint.Y + position.Y,
                0);
            Vector3 end = new Vector3(
                line.EndPoint.X + position.X,
                line.EndPoint.Y + position.Y,
                0);
            targetDxf.Entities.Add(new netDxf.Entities.Line(start, end) { Layer = layerWires });
        }

        foreach (var circle in sourceBlock.Entities.Circles)
        {
            Vector3 center = new Vector3(
                circle.Center.X + position.X,
                circle.Center.Y + position.Y,
                0);
            targetDxf.Entities.Add(new netDxf.Entities.Circle(center, circle.Radius) { Layer = layerWires });
        }

        targetDxf.Entities.Add(new netDxf.Entities.Text(designation,
            new Vector2(position.X - 12, position.Y - 5), 3.5)
        { Layer = layerFormat });
        targetDxf.Entities.Add(new netDxf.Entities.Text(article,
            new Vector2(position.X - 12, position.Y - 22), 2.5)
        { Layer = layerFormat });
    }

    // Вспомогательный метод прорисовки трех вертикальных силовых проводов 380В
    private void DrawHorizontalWires(DxfDocument dxf, double xStart, double y, double length, Layer layer)
    {
        double phaseStep = 10;
        for (int i = 0; i < 3; i++)
        {
            double cy = y - (i * phaseStep);
            dxf.Entities.Add(new netDxf.Entities.Line(
                new Vector3(xStart, cy, 0),
                new Vector3(xStart + length, cy, 0))
            { Layer = layer });
        }
    }
    private void DrawVerticalWires(DxfDocument dxf, double xStart, double yStart, double length, Layer layer)
    {
        double phaseStep = 10;
        for (int i = 0; i < 3; i++)
        {
            double cx = xStart + (i * phaseStep);
            dxf.Entities.Add(new netDxf.Entities.Line(
                new Vector3(cx, yStart, 0),
                new Vector3(cx, yStart - length, 0))
            { Layer = layer });
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
