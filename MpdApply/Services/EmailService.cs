using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MpdApply.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _http;
    private readonly ILogger<EmailService> _log;

    public EmailService(IConfiguration config, IHttpClientFactory http, ILogger<EmailService> log)
    {
        _config = config;
        _http = http;
        _log = log;
    }

    public async Task SendApplicationAsync(string recipientEmail, string applicantName, byte[] pdfBytes)
    {
        var apiKey = _config["SendGrid:ApiKey"];
        var fromEmail = _config["SendGrid:FromEmail"] ?? "noreply@applications.local";
        var fromName = _config["SendGrid:FromName"] ?? "MPD Applications";

        var base64Pdf = Convert.ToBase64String(pdfBytes);
        var subject = $"New Employment Application — {applicantName}";
        var body = $"""
            <p>A new employment application has been submitted.</p>
            <p><strong>Applicant:</strong> {applicantName}</p>
            <p>The completed application is attached as a PDF.</p>
            <hr/><p style="font-size:12px;color:#666;">Middletown Police Department — Online Application System</p>
            """;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _log.LogInformation("[Email stub] Would send application from {Applicant} to {Recipient}", applicantName, recipientEmail);
            return;
        }

        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = recipientEmail } } } },
            from = new { email = fromEmail, name = fromName },
            subject,
            content = new[] { new { type = "text/html", value = body } },
            attachments = new[]
            {
                new
                {
                    content = base64Pdf,
                    type = "application/pdf",
                    filename = $"Application_{applicantName.Replace(" ", "_")}.pdf",
                    disposition = "attachment"
                }
            }
        };

        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var response = await client.PostAsync(
            "https://api.sendgrid.com/v3/mail/send",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
            _log.LogError("SendGrid error {Status} sending application for {Applicant}", (int)response.StatusCode, applicantName);
    }
}
