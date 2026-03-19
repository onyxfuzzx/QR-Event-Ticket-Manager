using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace QREventPlatform.Advanced.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendTicketAsync(
        string toEmail,
        string eventName,
        string ticketCode,
        string qrUrl,
        CancellationToken ct = default
    )
    {
        // ============================
        // VALIDATE INPUT
        // ============================
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Email is required");

        if (!MailboxAddress.TryParse(toEmail, out var toMailbox))
            throw new ArgumentException("Invalid email format");

        // ============================
        // LOAD SMTP CONFIG
        // ============================
        var host = _config["Email:SmtpHost"];
        var portValue = _config["Email:SmtpPort"];
        var user = _config["Email:SmtpUser"];
        var pass = _config["Email:SmtpPass"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(portValue) ||
            string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(pass))
        {
            throw new InvalidOperationException("SMTP configuration is missing");
        }

        if (!int.TryParse(portValue, out var port))
            throw new InvalidOperationException("SMTP port is invalid");

        // ============================
        // HTML SAFE CONTENT
        // ============================
        var safeEventName = WebUtility.HtmlEncode(eventName);
        var safeTicketCode = WebUtility.HtmlEncode(ticketCode);
        var safeQrUrl = WebUtility.HtmlEncode(qrUrl);

        // ============================
        // BUILD EMAIL
        // ============================
        var message = new MimeMessage();
        message.From.Add(
            new MailboxAddress("QR Event Platform", user)
        );
        message.To.Add(toMailbox);
        message.Subject = $"🎫 Ticket for {safeEventName}";

        message.Body = new TextPart("html")
        {
            Text = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8fafc; padding: 40px 20px; color: #1e293b;"">
    <div style=""max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);"">
        <div style=""background-color: #6366f1; padding: 30px; text-align: center;"">
            <h2 style=""color: #ffffff; margin: 0; font-size: 24px;"">🎟️ Your Digital Ticket</h2>
        </div>
        <div style=""padding: 30px;"">
            <p style=""font-size: 16px; line-height: 1.6; margin-bottom: 24px;"">Hi there! Your ticket for <strong>{safeEventName}</strong> is ready. Please present the QR code below at the entrance.</p>
            
            <div style=""background-color: #f1f5f9; border-radius: 8px; padding: 20px; border: 1px dashed #cbd5e1; text-align: center; margin-bottom: 30px;"">
                <p style=""font-size: 14px; color: #64748b; margin: 0 0 8px 0; text-transform: uppercase; letter-spacing: 1px;"">Ticket Code</p>
                <code style=""font-size: 20px; font-weight: bold; color: #1e293b; letter-spacing: 2px;"">{safeTicketCode}</code>
            </div>

            <div style=""text-align: center;"">
                <a href=""{safeQrUrl}"" style=""display: inline-block; background-color: #6366f1; color: #ffffff; padding: 14px 28px; border-radius: 8px; text-decoration: none; font-weight: bold; font-size: 16px; box-shadow: 0 2px 4px rgba(99, 102, 241, 0.3);"">View QR Code</a>
            </div>
        </div>
        <div style=""background-color: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px; text-align: center;"">
            <p style=""font-size: 12px; color: #94a3b8; margin: 0;"">This is an automated message. Please do not reply.</p>
        </div>
    </div>
</div>
            "
        };

        // ============================
        // SEND EMAIL
        // ============================
        using var smtp = new SmtpClient();

        // ⚠️ Only disable certificate checks if explicitly configured (dev only)
        if (_config.GetValue<bool>("Email:DisableCertCheck"))
        {
            smtp.CheckCertificateRevocation = false;
        }

        await smtp.ConnectAsync(
            host,
            port,
            SecureSocketOptions.StartTls,
            ct
        );

        await smtp.AuthenticateAsync(user, pass, ct);
        await smtp.SendAsync(message, ct);
        await smtp.DisconnectAsync(true, ct);
    }

}
