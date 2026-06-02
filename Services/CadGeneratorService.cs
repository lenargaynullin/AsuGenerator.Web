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

        // 1. ЧЕРТИМ ВВОДНОЙ АВТОМАТ QF1 (Слева чертежа: X = 50, Y = 210)
        double currentX = 50;
        double currentY = 210;

        var qf1 = components.Find(c => c.Designation == "QF1");
        if (qf1 != null)
        {
            // Вертикальный провод питания сверху (длина 40 мм)
            DrawVerticalWires(sheet1, currentX, currentY + 40, 40, defaultLayer);
            // Вставляем блок УГО автомата из вашего файла qf_3p.dxf
            InsertComponentBlock(sheet1, "QF_3P", new Vector2(currentX, currentY), qf1.Designation, qf1.Article, defaultLayer, gostStyle);
            // Отвод провода вниз после автомата
            DrawVerticalWires(sheet1, currentX, currentY - 15, 40, defaultLayer);
        }

        // 2. АВТОМАТИЧЕСКАЯ РАССТАНОВКА ФИДЕРОВ ОБОГРЕВА ШУЭ В ЦИКЛЕ С СДВИГОМ ПО X
        // Считываем реальное количество линий из опросника ШУЭ (дефолт 5, как на вашем скрине)
        int outletsCount = config.OutletsHeatingCount > 0 ? config.OutletsHeatingCount : 5;

        double startX = 100; // Цепочку фидеров обогрева начинаем правее, с координаты X = 100
        double stepX = 35;   // Жесткий ГОСТ-шаг сдвига вправо для каждого фидера (35 мм)

        for (int i = 1; i <= outletsCount; i++)
        {
            double fX = startX + ((i - 1) * stepX);

            // Предохранитель: останавливаем цикл, чтобы чертеж не залез на область штампа (X_max = 220)
            if (fX > 220) break;

            // Ищем фидер в сформированной на сайте b2b-спецификации (QFD1, QFD2...)
            var qfd = components.Find(c => c.Designation == $"QFD{i}");
            if (qfd != null)
            {
                // Проводим вертикальный подвод проводов сверху к фидеру
                DrawVerticalWires(sheet1, fX, currentY + 40, 40, defaultLayer);

                // Вставляем блок диффавтомата (используем ваш файл qf_3p как графический атом)
                InsertComponentBlock(sheet1, "QF_3P", new Vector2(fX, currentY), qfd.Designation, qfd.Article, defaultLayer, gostStyle);

                // Проводим отвод провода вниз от автомата к клеммам
                DrawVerticalWires(sheet1, fX, currentY - 15, 40, defaultLayer);

                // Вставляем силовой выходной клеммник ТЕРМИНАЛ_2 в самом низу (Y = 80)
                InsertComponentBlock(sheet1, "TERMINAL_2", new Vector2(fX, 80), string.Format("X1.{0}", i), "ЗНИ-4", defaultLayer, gostStyle);
            }
        }

        schematics.Add(string.Format("Схема_Лист1_Сила_КП{0}.dxf", config.KpNumber), SaveToBytes(sheet1));

        // --- ЛИСТ 2: АВТОМАТИКА И ПЛК (A3_2.dxf) ---
        DxfDocument sheet2 = _blockManager.GetTemplate("A3_2");
        FillStamp(sheet2, config, isFirstSheet: false, gostStyle);
        schematics.Add(string.Format("Схема_Лист2_Автоматика_КП{0}.dxf", config.KpNumber), SaveToBytes(sheet2));

        return schematics;
    }


    private void FillStamp(DxfDocument dxf, VentAvtomatikaConfig config, bool isFirstSheet, TextStyle style)
    {
        Layer defaultLayer = dxf.Layers["0"];
        double stampEndX = 415;
        double stampStartY = 5;

        if (isFirstSheet)
        {
            double stampStartX = stampEndX - 185;
            // Принудительно задаем гостовский b2b-стиль текста для каждой ячейки штампа
            dxf.Entities.Add(new netDxf.Entities.Text(config.CompanyName.ToUpper(), new Vector2(stampStartX + 5, stampStartY + 5), 5) { Layer = defaultLayer, Style = style });
            dxf.Entities.Add(new netDxf.Entities.Text($"ЩУВ. КП № {config.KpNumber}", new Vector2(stampStartX + 5, stampStartY + 20), 5) { Layer = defaultLayer, Style = style });
            dxf.Entities.Add(new netDxf.Entities.Text("Разработал: AsuGenerator SaaS", new Vector2(stampStartX + 5, stampStartY + 45), 3.5) { Layer = defaultLayer, Style = style });
            dxf.Entities.Add(new netDxf.Entities.Text("1", new Vector2(stampEndX - 25, stampStartY + 15), 3.5) { Layer = defaultLayer, Style = style });
        }
        else
        {
            double smallStampStartX = stampEndX - 110;
            dxf.Entities.Add(new netDxf.Entities.Text($"ЩУВ. КП № {config.KpNumber}", new Vector2(smallStampStartX + 5, stampStartY + 5), 4) { Layer = defaultLayer, Style = style });
            dxf.Entities.Add(new netDxf.Entities.Text("2", new Vector2(stampEndX - 15, stampStartY + 5), 3.5) { Layer = defaultLayer, Style = style });
        }
    }


    // new

    // 1. В СИГНАТУРУ МЕТОДА ДОБАВЛЯЕМ ПЕРЕДАЧУ СТИЛЯ (TextStyle style)
    private void InsertComponentBlock(DxfDocument targetDxf, string blockName, Vector2 position, string designation, string article, Layer layer, TextStyle style)
    {
        try
        {
            string safeBlockName = blockName.ToLower().Trim();
            DxfDocument sourceBlock = _blockManager.GetBlock(safeBlockName);

            // 1. Копируем отрезки (Lines) с их родными слоями из файла блока
            foreach (var line in sourceBlock.Entities.Lines)
            {
                Vector3 startPos = line.StartPoint + new Vector3(position.X, position.Y, 0);
                Vector3 endPos = line.EndPoint + new Vector3(position.X, position.Y, 0);

                var newLine = new netDxf.Entities.Line(startPos, endPos);
                // Если слой есть в исходном файле, netDxf перенесет его сам безопасно
                if (line.Layer != null) newLine.Layer = line.Layer;

                targetDxf.Entities.Add(newLine);
            }

            // 2. Копируем окружности (Circles)
            foreach (var circle in sourceBlock.Entities.Circles)
            {
                Vector3 centerPos = circle.Center + new Vector3(position.X, position.Y, 0);
                var newCircle = new netDxf.Entities.Circle(centerPos, circle.Radius);
                if (circle.Layer != null) newCircle.Layer = circle.Layer;
                targetDxf.Entities.Add(newCircle);
            }

            // 3. Копируем полилинии (Polylines2D) — ФИКС: Убрано жесткое переопределение слоя "0"
            foreach (var polyline in sourceBlock.Entities.Polylines2D)
            {
                var clonePoly = (netDxf.Entities.Polyline2D)polyline.Clone();
                foreach (var vertex in clonePoly.Vertexes)
                {
                    vertex.Position = new Vector2(vertex.Position.X + position.X, vertex.Position.Y + position.Y);
                }
                // Оставляем родной слой полилинии из файла, чтобы избежать падения в catch
                targetDxf.Entities.Add(clonePoly);
            }
        }
        catch (Exception ex)
        {
            // Если блок все-таки упал, чертим ГОСТ-прямоугольник на слое "0", чтобы чертеж не остался пустым
            System.Diagnostics.Debug.WriteLine($"[AsuSaaS]: Ошибка блока {blockName}: {ex.Message}");
            double w = 15; double h = 10;
            targetDxf.Entities.Add(new netDxf.Entities.Line(position, new Vector2(position.X + w, position.Y)) { Layer = layer });
            targetDxf.Entities.Add(new netDxf.Entities.Line(new Vector2(position.X + w, position.Y), new Vector2(position.X + w, position.Y + h)) { Layer = layer });
            targetDxf.Entities.Add(new netDxf.Entities.Line(new Vector2(position.X + w, position.Y + h), new Vector2(position.X, position.Y + h)) { Layer = layer });
            targetDxf.Entities.Add(new netDxf.Entities.Line(new Vector2(position.X, position.Y + h), position) { Layer = layer });
        }

        // --- 4. МАРКИРОВКА ШРИФТОМ GOST Type BU ---
        var textDes = new netDxf.Entities.Text(designation, new Vector2(position.X - 12, position.Y + 2), 3.5)
        {
            Layer = layer,
            Style = style
        };
        targetDxf.Entities.Add(textDes);

        string shortValue = article.Contains("32А") || article.Contains("32A") ? "32А" :
                           (article.Contains("16А") || article.Contains("16A") ? "16А" : "25А");

        var textVal = new netDxf.Entities.Text(shortValue, new Vector2(position.X - 12, position.Y - 5), 2.5)
        {
            Layer = layer,
            Style = style
        };
        targetDxf.Entities.Add(textVal);
    }






    private void DrawVerticalWires(DxfDocument dxf, double xStart, double yStart, double length, Layer layer)
    {
        double phaseStep = 8;
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
