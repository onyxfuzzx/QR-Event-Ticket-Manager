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
                <h2>Your Ticket</h2>
                <p><b>Event:</b> {safeEventName}</p>
                <p><b>Code:</b> {safeTicketCode}</p>
                <p>
                    <a href='{safeQrUrl}'>Open QR Code</a>
                </p>
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
