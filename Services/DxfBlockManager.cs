using netDxf;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AsuGenerator.Web.Services;

public class DxfBlockManager
{
    private readonly Dictionary<string, DxfDocument> _blockCache = new();
    private readonly string _blocksFolder;

    public DxfBlockManager(IWebHostEnvironment env)
    {
        _blocksFolder = Path.Combine(env.ContentRootPath, "wwwroot", "Blocks");
    }

    // Метод извлечения мелких блоков (компонентов) из RAM
    public DxfDocument GetBlock(string blockName)
    {
        string fullPath = Path.Combine(_blocksFolder, $"{blockName}.dxf");

        if (!_blockCache.ContainsKey(blockName))
        {
            if (File.Exists(fullPath))
            {
                _blockCache[blockName] = DxfDocument.Load(fullPath);
            }
            else
            {
                throw new FileNotFoundException($"B2B DXF-блок не найден: {fullPath}");
            }
        }
        return _blockCache[blockName];
    }

    // Безопасный метод клонирования рамок А3
    public DxfDocument GetTemplate(string templateName)
    {
        string fullPath = Path.Combine(_blocksFolder, $"{templateName}.dxf");
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"B2B файл рамки не найден: {fullPath}");
        }

        // Загружаем оригинал
        var original = DxfDocument.Load(fullPath);

        // Создаем чистый новый документ той же версии
        DxfDocument clone = new DxfDocument(original.DrawingVariables.AcadVer);

        // Клонируем линии
        foreach (var ent in original.Entities.Lines)
            clone.Entities.Add((netDxf.Entities.Line)ent.Clone());

        // Клонируем текст
        foreach (var ent in original.Entities.Texts)
            clone.Entities.Add((netDxf.Entities.Text)ent.Clone());

        return clone;
    }
}