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
            message.To.Add(new MailboxAddress("Ленар Гайнуллин", SenderEmail));
            message.Subject = $"🔥 НОВЫЙ ЗАКАЗ ШКАФА: {DateTime.Now:dd.MM.yyyy HH:mm}";

            // Собираем данные для HTML
            int totalDi = cabinet.DiCount;
            int totalDo = cabinet.DoCount;
            int totalAi = cabinet.AiCount;
            int totalAo = cabinet.AoCount;
            int activeLinesCount = lines.Count(l => l.IsEnabled);

            // СОЗДАЕМ КРАСИВЫЙ B2B HTML-ШАБЛОН ПИСЬМА ПО ВСЕМ СИГНАЛАМ И КЛЕММАМ
            string htmlBody = $@"
    <div style='font-family: Arial, sans-serif; max-width: 650px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; background-color: #f8fafc;'>
        <div style='background-color: #1e293b; padding: 24px; text-align: center; border-bottom: 4px solid #f59e0b;'>
            <h2 style='color: #ffffff; margin: 0; font-size: 20px;'>🚀 ПОСТУПИЛ НОВЫЙ ЗАКАЗ ШКАФА</h2>
            <p style='color: #94a3b8; margin: 4px 0 0 0; font-size: 13px;'>Платформа: asugenerator.ru | Время: {DateTime.Now:dd.MM.yyyy HH:mm}</p>
        </div>
        <div style='padding: 24px;'>
            <div style='margin-bottom: 20px;'>
                <h3 style='color: #1e3a8a; margin: 0 0 8px 0; font-size: 15px; border-bottom: 1px solid #bfdbfe; padding-bottom: 4px;'>📦 ШАГ 1: Конструктив корпуса</h3>
                <table style='width: 100%; font-size: 13px; color: #334155; border-collapse: collapse;'>
                    <tr><td style='padding: 4px 0; font-weight: bold; width: 180px;'>Производитель / Тип:</td><td>{cabinet.Manufacturer} ({cabinet.MountType}, {cabinet.IpRating})</td></tr>
                    <tr><td style='padding: 4px 0; font-weight: bold;'>Габариты (ВхШхГ):</td><td style='color: #b45309; font-weight: bold;'>{dimensions.Replace("×", " х ")} мм</td></tr>
                    <tr><td style='padding: 4px 0; font-weight: bold;'>Цоколь / Вентилятор:</td><td>Цоколь: {cabinet.PlinthHeight} | Вентилятор: {cabinet.FanModel} ({cabinet.FanQuantity} шт.)</td></tr>
                    <tr><td style='padding: 4px 0; font-weight: bold;'>Доп. комплектация:</td><td style='font-size: 12px;'>Ручка: {(cabinet.HasDoorHandle ? "✅" : "❌")} | Карман: {(cabinet.HasPocket ? "✅" : "❌")} | Рейки: {(cabinet.HasDoorRails ? "✅" : "❌")} | Концевик: {(cabinet.HasLimitSwitch ? "✅" : "❌")} | Полка: {(cabinet.HasShelf ? "✅" : "❌")}</td></tr>
                </table>
            </div>
            <div style='margin-bottom: 20px;'>
                <h3 style='color: #065f46; margin: 0 0 8px 0; font-size: 15px; border-bottom: 1px solid #a7f3d0; padding-bottom: 4px;'>🧠 ШАГ 2: Контроллер и связь</h3>
                <table style='width: 100%; font-size: 13px; color: #334155;'>
                    <tr><td style='padding: 4px 0; font-weight: bold; width: 180px;'>Модель ПЛК / Протокол:</td><td>ОВЕН {cabinet.PlcType} ({cabinet.Protocol}, питание {cabinet.PlcPower}В)</td></tr>
                    <tr><td style='padding: 4px 0; font-weight: bold;'>Сигналы КИПиА:</td><td style='font-weight: bold; color: #047857;'>DI: {totalDi} | DO: {totalDo} | AI: {totalAi} | AO: {totalAo}</td></tr>
                </table>
            </div>
            <div style='margin-bottom: 20px;'>
                <h3 style='color: #9a3412; margin: 0 0 8px 0; font-size: 15px; border-bottom: 1px solid #ffedd5; padding-bottom: 4px;'>⚡ ШАГ 3: Силовая защита ({activeLinesCount} линий)</h3>
                <table style='width: 100%; font-size: 12px; border-collapse: collapse; text-align: left;'>
                    <tr style='background-color: #f1f5f9; font-weight: bold;'><th style='padding: 6px;'>ОУ</th><th style='padding: 6px;'>Тип</th><th style='padding: 6px;'>Полюса</th><th style='padding: 6px;'>Ток</th><th style='padding: 6px;'>Контактор</th></tr>";
            foreach (var line in lines.Where(l => l.IsEnabled))
            {
                string type = line.HasRCD ? "АВДТ" : "ВА";
                htmlBody += $"<tr style='border-bottom: 1px solid #e2e8f0;'><td style='padding: 6px; font-weight: bold;'>{line.Designation}</td><td style='padding: 6px;'>{type}</td><td style='padding: 6px;'>{line.Poles}</td><td style='padding: 6px;'>{line.Current} А ({line.Curve})</td><td style='padding: 6px;'>{(line.HasContactor ? "✅" : "❌")}</td></tr>";
            }
            htmlBody += $@"</table>
            </div>
            <div>
                <h3 style='color: #4338ca; margin: 0 0 8px 0; font-size: 15px; border-bottom: 1px solid #e0e7ff; padding-bottom: 4px;'>🔌 ШАГ 4: Клеммные ряды и монтаж</h3>
                <table style='width: 100%; font-size: 12px; border-collapse: collapse; text-align: left; margin-bottom: 12px;'>
                    <tr style='background-color: #f1f5f9; font-weight: bold;'><th style='padding: 6px;'>Ряд XT</th><th style='padding: 6px;'>Тип клеммы</th><th style='padding: 6px;'>Сечение</th><th style='padding: 6px;'>Кол-во</th></tr>";
            foreach (var term in terminals.Where(t => t.Quantity > 0))
            {
                htmlBody += $"<tr style='border-bottom: 1px solid #e2e8f0;'><td style='padding: 6px; font-weight: bold;'>{term.XBlockName}</td><td style='padding: 6px;'>{term.TerminalType}</td><td style='padding: 6px;'>{term.WireSection}</td><td style='padding: 6px;'>{term.Quantity} шт</td></tr>";
            }
            htmlBody += $@"</table>
                <table style='width: 100%; font-size: 13px; color: #334155;'>
                    <tr><td style='padding: 4px 0; font-weight: bold; width: 180px;'>Вендор клемм / Короб:</td><td>{cabinet.TerminalVendor} (Короб ПВХ: {cabinet.TrunkingSize}, резерв {cabinet.TerminalReservePercent}%)</td></tr>
                    <tr><td style='padding: 4px 0; font-weight: bold;'>Расходные материалы:</td><td>Авторасчет короба: {(cabinet.AutoCalculateTrunking ? "✅" : "❌")} | Провод ПуГВ/НШВИ: {(cabinet.IncludeWireAndFerrules ? "✅" : "❌")}</td></tr>
                </table>
            </div>
        </div>
        <div style='background-color: #1e293b; padding: 14px; text-align: center; font-size: 12px; color: #94a3b8;'>
            Лог лида asugenerator.ru | © 2026 Разработчик: Ленар Гайнуллин
        </div>
    </div>";

            // ИСПРАВЛЕНО: Меняем plain на html и передаем сформированную разметку
            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 5000;
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(SmtpServer, SmtpPort, true);
            await client.AuthenticateAsync(SenderEmail, SenderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        public async Task SendFeedbackAsync(string name, string contact, string type, string message)
        {
            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("AsuGenerator", SenderEmail));
            mail.To.Add(new MailboxAddress("Ленар", SenderEmail));
            mail.Subject = $"[{type}] {name} — отзыв с asugenerator.ru";

            var sb = new StringBuilder();
            sb.AppendLine($"Имя: {name}");
            sb.AppendLine($"Контакты: {contact}");
            sb.AppendLine($"Тип: {type}");
            sb.AppendLine();
            sb.AppendLine(message);

            mail.Body = new TextPart("plain") { Text = sb.ToString() };

            using var client = new SmtpClient();
            client.Timeout = 5000;
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(SmtpServer, SmtpPort, true);
            await client.AuthenticateAsync(SenderEmail, SenderPassword);
            await client.SendAsync(mail);
            await client.DisconnectAsync(true);
        }
    }
}
