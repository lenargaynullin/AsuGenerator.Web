using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsuGenerator.Web.Models;
using static AsuGenerator.Web.Components.Pages.Configurator;

namespace AsuGenerator.Web.Services
{
    public class EmailNotificationService
    {
        private const string SmtpServer = "smtp.mail.ru";
        private const int SmtpPort = 465; // Защищенный SSL порт Mail.ru
        private const string SenderEmail = "powerman@mail.ru";

        // ВАЖНО: Сюда вставляется 16-значный "Пароль для внешних приложений" из настроек Mail.ru
        private const string SenderPassword = "3zefQcjYhlGhlTH98Nkg";

        public async Task SendUniversalOrderAsync(BaseCabinetConfig cabinet, string dimensions, List<HeatingLine> lines, List<TerminalRow> terminals)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ASU Generator", SenderEmail));
            message.To.Add(new MailboxAddress("Ленар Гайнуллин", SenderEmail)); // Отправка самому себе
            message.Subject = $"🔥 НОВЫЙ ЗАКАЗ ШКАФА: {DateTime.Now:dd.MM.yyyy HH:mm}";

            var sb = new StringBuilder();
            sb.AppendLine("====================================================");
            sb.AppendLine("   ПОСТУПИЛ НОВЫЙ ПОКОМПОНЕНТНЫЙ ЗАКАЗ ШКАФА");
            sb.AppendLine("====================================================");
            sb.AppendLine();

            sb.AppendLine("--- ШАГ 1: КОНСТРУКТИВ КОРПУСА ---");
            sb.AppendLine($"• Производитель корпуса: {cabinet.Manufacturer}");
            sb.AppendLine($"• Тип монтажа: {cabinet.MountType}");
            sb.AppendLine($"• Степень защиты: {cabinet.IpRating}");
            sb.AppendLine($"• Габариты оболочки: {dimensions}");
            sb.AppendLine($"• Карман для документов: {(cabinet.HasPocket ? "ДА" : "НЕТ")}");
            sb.AppendLine($"• Боковые панели: {(cabinet.HasSidePanels ? "ДА" : "НЕТ")}");
            sb.AppendLine($"• Ручка двери с замком: {(cabinet.HasDoorHandle ? "ДА" : "НЕТ")}");
            sb.AppendLine($"• Внутренняя полка: {(cabinet.HasShelf ? "ДА" : "НЕТ")}");
            sb.AppendLine($"• Высота цоколя: {cabinet.PlinthHeight}");
            sb.AppendLine();

            sb.AppendLine("--- ШАГ 2: КОНТРОЛЛЕР И СВЯЗЬ ---");
            sb.AppendLine($"• Модель ПЛК ОВЕН: {cabinet.PlcType}");
            sb.AppendLine($"• Протокол диспетчеризации: {cabinet.Protocol}");
            sb.AppendLine();

            sb.AppendLine("--- ШАГ 3: ТАБЛИЦА СИЛОВОЙ ЗАЩИТЫ ---");
            var activeLines = lines.Where(l => l.IsEnabled).ToList();
            if (activeLines.Any())
            {
                foreach (var line in activeLines)
                {
                    string type = line.HasRCD ? "АВДТ" : "ВА";
                    sb.AppendLine($"  Линия {line.Designation} => {type}, Полюсов: {line.Poles}P, Номинал: {line.Current}A, Характеристика: {line.Curve}, Ток КЗ: {line.IkZ}кА, Контактор: {(line.HasContactor ? "ДА" : "НЕТ")}, Термостат: {(line.HasThermostat ? "ДА" : "НЕТ")}");
                }
            }
            else
            {
                sb.AppendLine("  [Линии защиты не выбраны]");
            }
            sb.AppendLine();

            sb.AppendLine("--- ШАГ 4: КЛЕММНЫЕ БЛОКИ И МОНТАЖ ---");
            if (terminals.Any())
            {
                foreach (var term in terminals)
                {
                    sb.AppendLine($"  Ряд {term.XBlockName} => Тип: {term.TerminalType}, Сечение: {term.WireSection} — Количество: {term.Quantity} шт.");
                }
                sb.AppendLine($"• Производитель клемм: {cabinet.TerminalVendor}");
                sb.AppendLine($"• Технологический резерв клемм: {cabinet.TerminalReservePercent}%");
                sb.AppendLine($"• Выбранный типоразмер короба: {cabinet.TrunkingSize}");
                sb.AppendLine($"• Авторасчет метража коробов по шкафу: {(cabinet.AutoCalculateTrunking ? "ДА" : "НЕТ")}");
                sb.AppendLine($"• Включить монтажный провод ПуГВ и НШВИ: {(cabinet.IncludeWireAndFerrules ? "ДА" : "НЕТ")}");
            }
            else
            {
                sb.AppendLine("  [Клеммные ряды пусты]");
            }
            sb.AppendLine();
            sb.AppendLine("====================================================");

            message.Body = new TextPart("plain") { Text = sb.ToString() };

            using var client = new SmtpClient();
            // Жесткий таймаут 5 секунд, чтобы сайт не зависал при сбоях связи
            client.Timeout = 5000;
            // Игнорируем сетевые ошибки SSL-сертификатов хостинга на Linux
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            // Подключаемся по разблокированному порту 465 через прямой SSL
            await client.ConnectAsync(SmtpServer, SmtpPort, true);
            await client.AuthenticateAsync(SenderEmail, SenderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
