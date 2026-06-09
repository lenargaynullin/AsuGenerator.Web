using System;
using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Services.Strategies;

/// <summary>
/// Фабрика для динамического подбора стратегии проектирования шкафа.
/// Использует основной конструктор C# 12+ (Решение IDE0290).
/// </summary>
public class CabinetStrategyFactory(IEnumerable<ICabinetStrategy> strategies)
{
    private readonly IEnumerable<ICabinetStrategy> _strategies = strategies;

    public ICabinetStrategy GetStrategy(string cabinetType)
    {
        // Поиск стратегии и упрощенная проверка на null через оператор ?? (Решение IDE0270)
        return _strategies.FirstOrDefault(s => s.CabinetType.Equals(cabinetType, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Тип шкафа '{cabinetType}' еще не поддерживается платформой.");
    }
}
