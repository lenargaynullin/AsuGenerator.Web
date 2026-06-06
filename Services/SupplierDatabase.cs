using System.Collections.Generic;
using System.Linq;
using AsuGenerator.Web.Models;

namespace AsuGenerator.Web.Services;

public class SupplierDatabase
{
    private readonly List<BreakerEntry> _breakers = new()
    {
        // KEAZ
        new() { Article = "21231DEK", Brand = "KEAZ", Poles = 3, RatedCurrent = 18, Curve = "C", IkZ = 6, Price = 4200m },
        new() { Article = "21224DEK", Brand = "KEAZ", Poles = 3, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 3100m },
        new() { Article = "12267DEK", Brand = "KEAZ", Poles = 1, RatedCurrent = 4,  Curve = "C", IkZ = 6, Price = 380m },
        new() { Article = "12280DEK", Brand = "KEAZ", Poles = 2, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 750m },
        new() { Article = "17014DEK", Brand = "KEAZ", Poles = 4, RatedCurrent = 32, Curve = "C", IkZ = 6, Price = 1150m },
        
        // EKF
        new() { Article = "EKF-3P-18A",  Brand = "EKF", Poles = 3, RatedCurrent = 18, Curve = "C", IkZ = 6, Price = 3900m },
        new() { Article = "EKF-3P-1A",   Brand = "EKF", Poles = 3, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 2900m },
        new() { Article = "EKF-1P-4A",   Brand = "EKF", Poles = 1, RatedCurrent = 4,  Curve = "C", IkZ = 6, Price = 350m },
        new() { Article = "EKF-2P-1A",   Brand = "EKF", Poles = 2, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 700m },
        new() { Article = "EKF-4P-32A",  Brand = "EKF", Poles = 4, RatedCurrent = 32, Curve = "C", IkZ = 6, Price = 1100m },
        
        // IEK
        new() { Article = "IEK-3P-18A",  Brand = "IEK", Poles = 3, RatedCurrent = 18, Curve = "C", IkZ = 6, Price = 3600m },
        new() { Article = "IEK-3P-1A",   Brand = "IEK", Poles = 3, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 2700m },
        new() { Article = "IEK-1P-4A",   Brand = "IEK", Poles = 1, RatedCurrent = 4,  Curve = "C", IkZ = 6, Price = 320m },
        new() { Article = "IEK-2P-1A",   Brand = "IEK", Poles = 2, RatedCurrent = 1,  Curve = "C", IkZ = 6, Price = 650m },
        new() { Article = "IEK-4P-32A",  Brand = "IEK", Poles = 4, RatedCurrent = 32, Curve = "C", IkZ = 6, Price = 1050m },
    };

    /// <summary>
    /// Ищет артикул автомата по параметрам и бренду.
    /// Если у бренда нет — ищет у любого.
    /// </summary>
    public string FindBreakerArticle(string brand, BreakerParams p)
    {
        // Сначала — нужный бренд
        var match = _breakers.FirstOrDefault(b =>
            b.Brand == brand &&
            b.Poles == p.Poles &&
            b.RatedCurrent >= p.RatedCurrent &&
            b.Curve == p.Curve &&
            b.IkZ >= p.IkZ);

        if (match != null) return match.Article;

        // Если нет — любой бренд
        match = _breakers.FirstOrDefault(b =>
            b.Poles == p.Poles &&
            b.RatedCurrent >= p.RatedCurrent &&
            b.Curve == p.Curve &&
            b.IkZ >= p.IkZ);

        return match?.Article ?? "";
    }

    public decimal FindBreakerPrice(string article)
    {
        return _breakers.FirstOrDefault(b => b.Article == article)?.Price ?? 1500m;
    }
}

public class BreakerEntry
{
    public string Article { get; set; } = "";
    public string Brand { get; set; } = "";
    public int Poles { get; set; }
    public double RatedCurrent { get; set; }
    public string Curve { get; set; } = "C";
    public int IkZ { get; set; } = 6;
    public decimal Price { get; set; }
}