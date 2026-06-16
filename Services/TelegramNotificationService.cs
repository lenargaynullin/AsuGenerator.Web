using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AsuGenerator.Web.Models;
using static AsuGenerator.Web.Components.Pages.Configurator;

namespace AsuGenerator.Web.Services
{
    public class TelegramNotificationService
    {
        private readonly HttpClient _http;

        // ВАЖНО: Замените эти строки на ваши реальные данные!
        // 1. Токен бота получаем у @BotFather в Telegram
        private const string BotToken = "8899796007:AAH0dTgqPHBsQaAO79w9N-cG87gAv79tUBU";
        // 2. Ваш личный Id получаем у бота @userinfobot
        private const string ChatId = "1439063506";

        public TelegramNotificationService(HttpClient http)
        {
            _http = http;
        }

        public async Task SendCabinetOrderToTelegramAsync(BaseCabinetConfig cabinet, string dimensions, List<HeatingLine> lines, List<TerminalRow> terminals)
        {
            try
            {
                string url = $"https://api.telegram.org/bot{BotToken}/sendMessage";

                var sb = new StringBuilder();
                sb.AppendLine("🔥 *НОВЫЙ ЗАКАЗ ШКАФА (ПОКОМПОНЕНТНЫЙ)*");
                sb.AppendLine($"• *Вендор корпуса:* {cabinet.Manufacturer} ({dimensions})");
                sb.AppendLine($"• *Монтаж:* {cabinet.MountType} | *Защита:* {cabinet.IpRating}");
                sb.AppendLine($"• *Опции:* Карман={(cabinet.HasPocket ? "✅" : "❌")}, Ручка={(cabinet.HasDoorHandle ? "✅" : "❌")}, Полка={(cabinet.HasShelf ? "✅" : "❌")}");
                sb.AppendLine($"• *Цоколь:* {cabinet.PlinthHeight}");
                sb.AppendLine();
                sb.AppendLine("🧠 *КОНТРОЛЛЕР*");
                sb.AppendLine($"• *Модель:* {cabinet.PlcType}");
                sb.AppendLine($"• *Протокол:* {cabinet.Protocol}");
                sb.AppendLine();
                sb.AppendLine("⚡ *ТАБЛИЦА АВТОМАТОВ*");
                var activeLines = lines.Where(l => l.IsEnabled).ToList();
                if (activeLines.Any())
                {
                    foreach (var line in activeLines)
                        sb.AppendLine($"• `{line.Designation}`: {line.Current}A, {line.Poles}P, Кривая {line.Curve} | Диф={(line.HasRCD ? "✅" : "❌")}, КМ={(line.HasContactor ? "✅" : "❌")}");
                }
                else
                    sb.AppendLine("_Силовые линии не добавлены_");
                sb.AppendLine();
                sb.AppendLine("🔌 *КЛЕММНЫЕ БЛОКИ*");
                if (terminals.Any())
                {
                    foreach (var term in terminals)
                        sb.AppendLine($"• `{term.XBlockName}`: {term.TerminalType}, {term.WireSection} — *{term.Quantity} шт.*");
                }
                else
                    sb.AppendLine("_Клеммные ряды пусты_");

                var payload = new
                {
                    chat_id = ChatId,
                    text = sb.ToString(),
                    parse_mode = "Markdown"
                };

                var response = await _http.PostAsJsonAsync(url, payload);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[Telegram API Error]: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Telegram Service Exception]: {ex.Message}");
            }
        }
    }
}
