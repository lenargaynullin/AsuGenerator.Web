using System;
using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Services;

public class CabinetStrategyFactory
{
    private readonly IEnumerable<ICabinetStrategy> _strategies;

    public CabinetStrategyFactory(IEnumerable<ICabinetStrategy> strategies)
    {
        _strategies = strategies;
    }

    public ICabinetStrategy GetStrategy(string cabinetType)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CabinetType.Equals(cabinetType, StringComparison.OrdinalIgnoreCase));

        if (strategy == null)
        {
            throw new ArgumentException($"Тип шкафа '{cabinetType}' еще не поддерживается платформой.");
        }

        return strategy;
    }
}
