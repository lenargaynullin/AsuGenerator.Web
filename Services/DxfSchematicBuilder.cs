using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Tables;
using System;
using System.Collections.Generic;

namespace AsuGenerator.Web.Services;

public class DxfSchematicBuilder
{
    private readonly DxfBlockManager _blockManager;

    public DxfSchematicBuilder(DxfBlockManager blockManager)
    {
        _blockManager = blockManager;
    }

    public void BuildVerticalFeeder(DxfDocument dxf, double startX, double topY,
        List<CadFeederItem> items, Layer layer, TextStyle style)
    {
        double currentY = topY;
        double wireLength = 15;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];

            // Провода сверху (только для первого)
            if (i == 0)
                DrawWires(dxf, startX, currentY + 10, wireLength, layer, item.WireCount);

            // Блок с клонированием
            InsertBlock(dxf, item.BlockName, startX, currentY,
                item.Designation, item.Article ?? "", layer, style, item.Quantity);

            // Провода снизу (только для последнего)
            if (i == items.Count - 1)
                DrawWires(dxf, startX, currentY - 15, wireLength, layer, item.WireCount);

            currentY -= item.VerticalSpace;
        }
    }

    /// <summary>
    /// Вставляет блок с клонированием из исходного DXF-файла.
    /// </summary>
    private void InsertBlock(DxfDocument targetDxf, string blockName, double x, double y,
        string designation, string article, Layer layer, TextStyle style, int qty)
    {
        try
        {
            string safeBlockName = blockName.ToLower().Trim();
            DxfDocument sourceDxf = _blockManager.GetBlock(safeBlockName);

            // 1. Клонируем определение блока
            Block blockDef;

            if (sourceDxf.Blocks.Contains(safeBlockName))
            {
                blockDef = (Block)sourceDxf.Blocks[safeBlockName].Clone();
                blockDef.Name = safeBlockName;
            }
            else
            {
                blockDef = new Block(safeBlockName);
                foreach (var entity in sourceDxf.Entities.All)
                {
                    if (entity.Clone() is EntityObject clone)
                        blockDef.Entities.Add(clone);
                }
            }

            // 2. Добавляем блок в целевой документ (если ещё нет)
            if (!targetDxf.Blocks.Contains(safeBlockName))
            {
                targetDxf.Blocks.Add(blockDef);
            }
            else
            {
                blockDef = targetDxf.Blocks[safeBlockName];
            }

            // 3. Вставляем Insert
            for (int i = 0; i < qty; i++)
            {
                Vector2 insertPoint = new Vector2(x + (i * 8), y);
                var insert = new Insert(blockDef, insertPoint) { Layer = layer };
                targetDxf.Entities.Add(insert);
            }

            // 4. Маркировка текстом
            var textDes = new Text(designation, new Vector2(x - 12, y + 2), 3.5)
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

    /// <summary>
    /// Рисует вертикальные провода.
    /// </summary>
    private void DrawWires(DxfDocument dxf, double x, double y, double length, Layer layer, int count)
    {
        double step = 8;
        for (int i = 0; i < count; i++)
        {
            double cx = x + (i * step);
            dxf.Entities.Add(new Line(new Vector2(cx, y), new Vector2(cx, y - length)) { Layer = layer });
        }
    }
}

/// <summary>
/// Модель строки силовой цепочки.
/// </summary>
public class CadFeederItem
{
    public string Designation { get; set; } = "";
    public string BlockName { get; set; } = "";
    public string Article { get; set; } = "";
    public double VerticalSpace { get; set; } = 40;
    public int WireCount { get; set; } = 4;
    public int Quantity { get; set; } = 1;
}