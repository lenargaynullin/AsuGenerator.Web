using System;
using System.Collections.Generic;
using System.Linq;

namespace AsuGenerator.Web.Services;

public class PriceCalculationService
{
    // Быстрая база данных цен в RAM: Ключ — Артикул, Значение — Базовая цена в рублях без НДС
    private static readonly Dictionary<string, decimal> PriceDatabase = new(StringComparer.OrdinalIgnoreCase)
{
    // --- Автоматика ОВЕН ---
    { "ПЧВЗ-11K-B", 34500.00m },         // Преобразователь частоты 11,0 кВт
    { "ПР200-24.4.2.0", 11200.00m },       // Программируемое реле с дисплеем
    { "ПРМ-24.1", 6200.00m },             // Модуль расширения ПЛК
    { "БП60Б-Д4-24", 3800.00m },          // Блок питания 60 Вт 24 В

    // --- Коммутация КИППРИБОР ---
    { "MR-203D", 450.00m },               // Промежуточное реле 24 VDC
    { "PYF-022BE/2", 280.00m },           // Колодка монтажная реле
    { "BS 2/15P", 35.00m },               // Зажим пластмассовый удерживающий
    { "LM-CF 24 V AC/DC", 120.00m },      // Модуль LED-индикации 24В
    { "MR-207A", 480.00m },               // Промежуточное реле 220 VAC
    { "LM-EN 230 V AC/DC", 130.00m },     // Модуль LED-индикации 230В

    // --- Силовое оборудование Dekraft ---
    { "18053DEK", 1850.00m },             // Контактор модульный 4НО 16 А
    { "18050DEK", 1400.00m },             // Контактор модульный 2НО 16 А
    { "21231DEK", 4200.00m },             // Автомат защиты двигателя ВА-431 (13-18 А)
    { "21224DEK", 3100.00m },             // Автомат защиты двигателя ВА-431 (0.63-1 А)
    { "21269DEK", 650.00m },              // Контакт дополнительный фронтальный
    { "12267DEK", 380.00m },              // Автоматический выключатель 1Р 4 А
    { "12280DEK", 750.00m },              // Автоматический выключатель 2Р 1 А
    { "17014DEK", 1150.00m },             // Выключатель-разъединитель 4P 32 А

    // --- Светосигнальная арматура и клеммы IEK ---
    { "BSW60-BD-3-K02", 420.00m },        // Переключатель 3 позиции LAY5
    { "BDK11", 110.00m },                 // Контактный блок 1НЗ
    { "BBT61-BA-K04", 250.00m },          // Кнопка управления LAY5 красная
    { "BBT50-BW-K06", 380.00m },          // Кнопка с подсветкой зеленая
    { "BLS10-ADDS-230-K05", 190.00m },    // Лампа 22мм желтая 230В
    { "BLS10-ADDS-230-K06", 190.00m },    // Лампа 22мм зеленая 230В
    { "BLS10-ADDS-230-K04", 190.00m },    // Лампа 22мм красная 230В
    { "YZN30-016-K03", 140.00m },         // Клемма винтовая КВИ-16 мм2
    { "YZN30-002-K03", 35.00m },          // Клемма винтовая КВИ-2,5 мм2
    { "YZN30-004D-K03", 85.00m },         // Клемма винтовая КВИ-4-21 двухуровневая
    { "YZN30D-ZGL-016-K03", 15.00m },     // Заглушка для КВИ-16 мм2
    { "YZN30D-ZGL-002-K03", 10.00m },     // Заглушка для КВИ-2,5 мм2
    { "YZN30D-ZGL-004D-K03", 20.00m }      // Заглушка для КВИ-4-21
};



    // Дефолтная цена на случай, если артикула еще нет в базе данных (чтобы расчет не падал)
    private const decimal DefaultBasePrice = 1500.00m;

    // Измените сигнатуру метода — добавляем decimal margin
    public CommercialProposal CalculateProposal(List<SelectedComponent> matchedComponents, VentAvtomatikaConfig? config, decimal margin)
    {
        var proposal = new CommercialProposal
        {
            ProjectName = config != null ? $"ШУВ для КП №{config.KpNumber}" : "Шкаф автоматизации ШУВ-ОВЕН (Золотой)",
            ClientName = config != null ? config.ClientName : "ООО ВентАвтоматика",
            AssemblyPrice = 45000.00m
        };

        foreach (var comp in matchedComponents)
        {
            if (!PriceDatabase.TryGetValue(comp.Article ?? string.Empty, out decimal basePrice))
            {
                var matchedKey = PriceDatabase.Keys.FirstOrDefault(k =>
                    (comp.Description ?? string.Empty).Contains(k, StringComparison.OrdinalIgnoreCase));
                basePrice = matchedKey != null ? PriceDatabase[matchedKey] : DefaultBasePrice;
            }

            proposal.Items.Add(new ProposalItem
            {
                Name = comp.Description,
                Quantity = comp.Quantity,  // ← БЕРЁМ НАПРЯМУЮ ИЗ КОМПОНЕНТА
                BasePrice = basePrice,
                MarginMultiplier = margin
            });
        }

        return proposal;
    }

}
