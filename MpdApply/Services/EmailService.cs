using System.Net;
using System.Net.Mail;

namespace MpdApply.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _log;

    public EmailService(IConfiguration config, ILogger<EmailService> log)
    {
        _config = config;
        _log = log;
    }

    public async Task SendApplicationAsync(string recipientEmail, string applicantName, byte[] pdfBytes)
    {
        var host = _config["Smtp:Host"];
        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
        var user = _config["Smtp:Username"];
        var pass = _config["Smtp:Password"];
        var from = _config["Smtp:FromEmail"] ?? user;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user))
        {
            _log.LogInformation("[Email stub] Submission from {Applicant} saved to database. Configure Smtp settings to enable email delivery.", applicantName);
            return;
        }

        var subject = $"New Employment Application — {applicantName}";
        var body = $"""
            <p>A new employment application has been submitted.</p>
            <p><strong>Applicant:</strong> {applicantName}</p>
            <p>The completed application is attached as a PDF.</p>
            <hr/><p style="font-size:12px;color:#666;">Middletown Police Department — Online Application System</p>
            """;

        using var message = new MailMessage();
        message.From = new MailAddress(from!, "MPD Applications");
        message.To.Add(recipientEmail);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;
        message.Attachments.Add(new Attachment(
            new MemoryStream(pdfBytes),
            $"Application_{applicantName.Replace(" ", "_")}.pdf",
            "application/pdf"));

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, pass)
        };

        try
        {
            await client.SendMailAsync(message);
            _log.LogInformation("Application email sent for {Applicant} to {Recipient}", applicantName, recipientEmail);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to send email for {Applicant}", applicantName);
            throw;
        }
    }
}
