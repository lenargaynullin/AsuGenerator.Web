using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Text;
using System.Threading.Tasks;
using AsuGenerator.Web.Models;
using static AsuGenerator.Web.Components.Pages.Configurator;

namespace AsuGenerator.Web.Services
{
    public class EmailNotificationService
    {
        private const string SmtpServer = "smtp.mail.ru";
        private const int SmtpPort = 587; // SSL порт Mail.ru
        private const string SenderEmail = "powerman@mail.ru";

        // ВАЖНО: Для Mail.ru нужен не обычный пароль от почты, 
        // а специальный "Пароль для внешних приложений" из настроек безопасности Mail.ru!
        private const string SenderPassword = "3zefQcjYhlGhlTH98Nkg";

        public async Task SendProjectOrderAsync(BaseCabinetConfig cabinet, string dimensions, List<HeatingLine> lines, List<TerminalRow> terminals)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("ASU Generator Робот", SenderEmail));
            message.To.Add(new MailboxAddress("Ленар Гайнуллин", SenderEmail)); // Отправляем сами себе
            message.Subject = $"🔥 НОВЫЙ ЗАКАЗ ШКАФА: {DateTime.Now:dd.MM.yyyy HH:mm}";

            // Формируем красивое текстовое тело письма с контекстом проекта
            var sb = new StringBuilder();
            sb.AppendLine("=== ДАННЫЕ ОБОЛОЧКИ (ШАГ 1) ===");
            sb.AppendLine($"Вендор корпуса: {cabinet.Manufacturer}");
            sb.AppendLine($"Тип монтажа: {cabinet.MountType}");
            sb.AppendLine($"Степень защиты: {cabinet.IpRating}");
            sb.AppendLine($"Выбранные габариты: {dimensions}");
            sb.AppendLine($"Карман для док.: {(cabinet.HasPocket ? "ДА" : "НЕТ")}");
            sb.AppendLine($"Боковые панели: {(cabinet.HasSidePanels ? "ДА" : "НЕТ")}");
            sb.AppendLine($"Ручка двери: {(cabinet.HasDoorHandle ? "ДА" : "НЕТ")}");
            sb.AppendLine($"Полка: {(cabinet.HasShelf ? "ДА" : "НЕТ")}");
            sb.AppendLine($"Цоколь: {cabinet.PlinthHeight}");
            sb.AppendLine($"Вентилятор: {cabinet.FanModel}");
            sb.AppendLine();

            sb.AppendLine("=== СИГНАЛЫ КИП (ШАГ 2) ===");
            sb.AppendLine($"ПЛК: {cabinet.PlcType}");
            sb.AppendLine($"Протокол: {cabinet.Protocol}");
            sb.AppendLine();

            sb.AppendLine("=== ТАБЛИЦА АВТОМАТОВ (ШАГ 3) ===");
            foreach (var line in lines)
            {
                sb.AppendLine($" Линия {line.Designation}: Вкл={line.IsEnabled}, Диф={line.HasRCD}, Полюсов={line.Poles}P, Ток={line.Current}A, Хар-ка={line.Curve}, Ток КЗ={line.IkZ}кА, Контактор={line.HasContactor}, Термостат={line.HasThermostat}");
            }
            sb.AppendLine();

            sb.AppendLine("=== ТАБЛИЦА КЛЕММНИКОВ (ШАГ 4) ===");
            foreach (var term in terminals)
            {
                sb.AppendLine($" Ряд {term.XBlockName}: Тип={term.TerminalType}, Сечение={term.WireSection}, Количество={term.Quantity} шт.");
            }
            sb.AppendLine();
            sb.AppendLine($"Вендор клемм: {cabinet.TerminalVendor}, Резерв: {cabinet.TerminalReservePercent}%");
            sb.AppendLine($"Авторасчет коробов: {(cabinet.AutoCalculateTrunking ? "ДА" : "НЕТ")}");
            sb.AppendLine($"Включить ПуГВ/НШВИ: {(cabinet.IncludeWireAndFerrules ? "ДА" : "НЕТ")}");

            message.Body = new TextPart("plain") { Text = sb.ToString() };

            using var client = new SmtpClient();
            // 1. Игнорируем сетевые проверки сертификатов хостинга на Linux
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            // 2. ИСПРАВЛЕНО: Подключаемся к порту 587 через STARTTLS (заменили true на StartTls)
            await client.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);

            // 3. Авторизация и отправка
            await client.AuthenticateAsync(SenderEmail, SenderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
