using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Tables;
using System.Collections.Generic;
using System.IO;
namespace AsuGenerator.Web.Services;

public class CadGeneratorService
{
    private readonly DxfBlockManager _blockManager;

    public CadGeneratorService(DxfBlockManager blockManager)
    {
        _blockManager = blockManager;
    }

    // Главный метод генерации чертежей проекта (Многолистовая b2b-генерация ШУЭ в цикле)
    public Dictionary<string, byte[]> GenerateProjectSchematics(List<SelectedComponent> components, VentAvtomatikaConfig config)
    {
        // --- 1. РЕГИСТРАЦИЯ ГОСТОВСКОГО ШРИФТА В ДОКУМЕНТЕ ---
        // Создаем стиль текста с именем "GOST_BU", привязанный к TTF-файлу шрифта
        TextStyle gostStyle = new TextStyle("GOST Type BU", "gost-type-bu.ttf");    

        var schematics = new Dictionary<string, byte[]>();

        // --- ЛИСТ 1: СИЛОВАЯ ЧАСТЬ (A3_1.dxf — точка 0,0 внешний угол) ---
        DxfDocument sheet1 = _blockManager.GetTemplate("A3_1");
        FillStamp(sheet1, config, isFirstSheet: true, gostStyle);

        // Если стиля с таким именем еще нет в таблице стилей подложки — добавляем его
        if (!sheet1.TextStyles.Contains(gostStyle.Name))
        {
            sheet1.TextStyles.Add(gostStyle);
        }

        // Базовый слой "0", который гарантированно есть в любом файле подложки
        Layer defaultLayer = sheet1.Layers["0"];




        // XT1
        double currentX = 50;
        double currentY = 250;

        var xt1 = components.Find(c => c.Designation == "XT1");
        if (xt1 != null)
        {
            // Вставляем блок УГО
            InsertComponentBlock(sheet1, "terminal", new Vector2(currentX, currentY), xt1.Designation, xt1.Article, defaultLayer, gostStyle, 4);

            // Отвод провода вниз
            DrawVerticalWires(sheet1, currentX, currentY - 4, 20, defaultLayer, 4);
        }
        currentY = currentY - 24;

        // QS1
        var qs1 = components.Find(c => c.Designation == "QS1");
        if (qs1 != null)
        {
            // Вставляем блок УГО
            InsertComponentBlock(sheet1, "qs_4p", new Vector2(currentX, currentY), qs1.Designation, qs1.Article, defaultLayer, gostStyle, 1);

            // Отвод провода вниз
            DrawVerticalWires(sheet1, currentX, currentY - 8, 20, defaultLayer, 4);
        }


        schematics.Add(string.Format("Схема_Лист1_Сила_КП{0}.dxf", config.KpNumber), SaveToBytes(sheet1));

        /*
        // --- ЛИСТ 2: АВТОМАТИКА И ПЛК (A3_2.dxf) ---
        DxfDocument sheet2 = _blockManager.GetTemplate("A3_2");
        FillStamp(sheet2, config, isFirstSheet: false, gostStyle);
        schematics.Add(string.Format("Схема_Лист2_Автоматика_КП{0}.dxf", config.KpNumber), SaveToBytes(sheet2));
        */

        return schematics;
    }


    private void FillStamp(DxfDocument dxf, VentAvtomatikaConfig config, bool isFirstSheet, TextStyle style)
    {
        Layer defaultLayer = dxf.Layers["0"];
        double stampEndX = 415;
        double stampStartY = 5;

        if (isFirstSheet)
        {
            double stampStartX = stampEndX - 185; // 230
            

            dxf.Entities.Add(new netDxf.Entities.Text(config.DocDesignation.ToUpper(), new Vector2(stampStartX + 107, stampStartY + 45), 3.5) { Layer = defaultLayer, Style = style }); // КУВФ.421417.215
            dxf.Entities.Add(new netDxf.Entities.Text(config.CabinetName, new Vector2(stampStartX + 77, stampStartY + 31), 2.5) { Layer = defaultLayer, Style = style }); // Шкаф управления вентиляцией
            dxf.Entities.Add(new netDxf.Entities.Text(config.ProjectNumber.ToUpper(), new Vector2(stampStartX + 71, stampStartY + 21), 2.5) { Layer = defaultLayer, Style = style }); // ОВИК-ШУВ1.2-1-8.0-0-0-0-1-20-1.0-00
            dxf.Entities.Add(new netDxf.Entities.Text(config.CompanyName.ToUpper(), new Vector2(stampStartX + 145, stampStartY + 5), 3.5) { Layer = defaultLayer, Style = style }); // AsuGenerator
            
            dxf.Entities.Add(new netDxf.Entities.Text("Гайнуллин Л.Т.", new Vector2(stampStartX + 18, stampStartY + 26), 2) { Layer = defaultLayer, Style = style }); // Разработал
            dxf.Entities.Add(new netDxf.Entities.Text("Гайнуллин Л.Т.", new Vector2(stampStartX + 18, stampStartY + 21), 2) { Layer = defaultLayer, Style = style }); // Проверил
            dxf.Entities.Add(new netDxf.Entities.Text("Гайнуллин Л.Т.", new Vector2(stampStartX + 18, stampStartY + 11), 2) { Layer = defaultLayer, Style = style }); // Н. контр.
            dxf.Entities.Add(new netDxf.Entities.Text("Гайнуллин Л.Т.", new Vector2(stampStartX + 18, stampStartY + 6), 2) { Layer = defaultLayer, Style = style }); // Утвердил

            string currentShortDate = DateTime.Now.ToString("dd.MM.yy");
            dxf.Entities.Add(new netDxf.Entities.Text(currentShortDate, new Vector2(stampStartX + 55, stampStartY + 26), 2) { Layer = defaultLayer, Style = style }); // дата
            dxf.Entities.Add(new netDxf.Entities.Text(currentShortDate, new Vector2(stampStartX + 55, stampStartY + 21), 2) { Layer = defaultLayer, Style = style }); // дата
            dxf.Entities.Add(new netDxf.Entities.Text(currentShortDate, new Vector2(stampStartX + 55, stampStartY + 11), 2) { Layer = defaultLayer, Style = style }); // дата
            dxf.Entities.Add(new netDxf.Entities.Text(currentShortDate, new Vector2(stampStartX + 55, stampStartY + 6), 2) { Layer = defaultLayer, Style = style }); // дата

            dxf.Entities.Add(new netDxf.Entities.Text("1", new Vector2(stampStartX + 148, stampStartY + 16), 2) { Layer = defaultLayer, Style = style }); // Номер листа
        }
        else
        {
            double smallStampStartX = stampEndX - 110;
            dxf.Entities.Add(new netDxf.Entities.Text(config.ProjectNumber.ToUpper(), new Vector2(smallStampStartX + 5, stampStartY + 5), 3.5) { Layer = defaultLayer, Style = style });
            dxf.Entities.Add(new netDxf.Entities.Text("2", new Vector2(stampEndX - 15, stampStartY + 5), 3.5) { Layer = defaultLayer, Style = style });
        }
    }


    // new

    // 1. В СИГНАТУРУ МЕТОДА ДОБАВЛЯЕМ ПЕРЕДАЧУ СТИЛЯ (TextStyle style)
    private void InsertComponentBlock(DxfDocument targetDxf, string blockName, Vector2 position,
    string designation, string article, Layer layer, TextStyle style, int numberOfComponent)
    {
        try
        {
            string safeBlockName = blockName.ToLower().Trim();
            DxfDocument sourceDxf = _blockManager.GetBlock(safeBlockName);

            // 1. Клонируем определение блока из исходного файла
            Block blockDef;

            if (sourceDxf.Blocks.Contains(safeBlockName))
            {
                // Блок уже определён как Block в исходном DXF
                blockDef = (Block)sourceDxf.Blocks[safeBlockName].Clone();
                blockDef.Name = safeBlockName;
            }
            else
            {
                // Создаём блок из сущностей (как раньше)
                blockDef = new Block(safeBlockName);
                foreach (var entity in sourceDxf.Entities.All)
                {
                    if (entity.Clone() is EntityObject clone)
                        blockDef.Entities.Add(clone);
                }
            }

            // 2. Добавляем определение блока в целевой документ (если ещё нет)
            if (!targetDxf.Blocks.Contains(safeBlockName))
            {
                targetDxf.Blocks.Add(blockDef);
            }
            else
            {
                blockDef = targetDxf.Blocks[safeBlockName];
            }

            // 3. Вставляем блок
            for (int i = 0; i < numberOfComponent; i++)
            {
                Vector2 insertPoint = new Vector2(position.X + (i * 8), position.Y);
                var insert = new Insert(blockDef, insertPoint)
                {
                    Layer = layer
                };
                targetDxf.Entities.Add(insert);
            }

            // 4. Маркировка
            var textDes = new netDxf.Entities.Text(designation,
                new Vector2(position.X - 12, position.Y + 2), 3.5)
            {
                Layer = layer,
                Style = style
            };
            targetDxf.Entities.Add(textDes);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AsuSaaS] Ошибка вставки блока {blockName}: {ex.Message}");
        }
    }

    private void DrawVerticalWires(DxfDocument dxf, double xStart, double yStart, double length, Layer layer, int numberOfLines)
    {
        double phaseStep = 8;
        for (int i = 0; i < numberOfLines; i++)
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
