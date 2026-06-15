using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AsuGenerator.Web.Services;

public class EplanXmlGenerator
{
    public byte[] Generate(List<SelectedComponent> components, string projectName)
    {
        var partsList = new EplanPartsList
        {
            RecordCount = components.Count,
            ProjectId = "62",
            PropertyDescriptions = new List<EplanPropertyDescription>
            {
                new() { Name = "P_ARTICLEREF_PARTNO", Context = "part" },
                new() { Name = "P_ARTICLEREF_COUNT", Context = "part" },
                new() { Name = "P_ARTICLE_TYPENR", Context = "part" },
                new() { Name = "P_ARTICLE_ORDERNR", Context = "part" },
                new() { Name = "P_ARTICLE_DESCR1", Context = "part" },
                new() { Name = "P_ARTICLE_MANUFACTURER", Context = "part" },
                new() { Name = "P_ARTICLE_SUPPLIER", Context = "part" }
            },
            Devices = new List<EplanDevice>()
        };

        foreach (var comp in components)
        {
            partsList.Devices.Add(new EplanDevice
            {
                FuncCategoryId = "100/6/1",
                IdentName = "=+++",
                Part = new EplanPart
                {
                    PartNumber = comp.Article ?? "",
                    Count = comp.Quantity > 0 ? comp.Quantity : 1,
                    TypeNumber = comp.Designation ?? "",
                    OrderNumber = comp.Article ?? "",
                    Description = $"ru_RU@{comp.Description ?? ""}",
                    Manufacturer = comp.Vendor ?? "",
                    Supplier = comp.Vendor ?? ""
                }
            });
        }

        using var memoryStream = new MemoryStream();
        var serializer = new XmlSerializer(typeof(EplanPartsList));
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("", "");
        serializer.Serialize(memoryStream, partsList, namespaces);
        return memoryStream.ToArray();
    }
}

[XmlRoot("partsList")]
public class EplanPartsList
{
    [XmlAttribute("RECORD_COUNT")]
    public int RecordCount { get; set; }

    [XmlAttribute("PROJECT_ID")]
    public string ProjectId { get; set; } = "62";

    [XmlArray("propertyDescriptions")]
    [XmlArrayItem("propertyDescription")]
    public List<EplanPropertyDescription> PropertyDescriptions { get; set; } = new();

    [XmlElement("device")]
    public List<EplanDevice> Devices { get; set; } = new();
}

public class EplanPropertyDescription
{
    [XmlAttribute("name")]
    public string Name { get; set; } = "";

    [XmlAttribute("context")]
    public string Context { get; set; } = "";
}

public class EplanDevice
{
    [XmlAttribute("P_FUNC_CATEGORY_GROUP_ID")]
    public string FuncCategoryId { get; set; } = "100/6/1";

    [XmlAttribute("P_ARTICLEREF_IDENTNAME")]
    public string IdentName { get; set; } = "=+++";

    [XmlElement("part")]
    public EplanPart Part { get; set; } = new();
}

public class EplanPart
{
    [XmlAttribute("P_ARTICLEREF_PARTNO")]
    public string PartNumber { get; set; } = "";

    [XmlAttribute("P_ARTICLEREF_COUNT")]
    public int Count { get; set; } = 1;

    [XmlAttribute("P_ARTICLE_TYPENR")]
    public string TypeNumber { get; set; } = "";

    [XmlAttribute("P_ARTICLE_ORDERNR")]
    public string OrderNumber { get; set; } = "";

    [XmlAttribute("P_ARTICLE_DESCR1")]
    public string Description { get; set; } = "";

    [XmlAttribute("P_ARTICLE_MANUFACTURER")]
    public string Manufacturer { get; set; } = "";

    [XmlAttribute("P_ARTICLE_SUPPLIER")]
    public string Supplier { get; set; } = "";
}