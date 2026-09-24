using System.Text.Json;

namespace MpdApply.Models;

public class FormSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FormTemplateId { get; set; }
    public FormTemplate? FormTemplate { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public string? IpAddress { get; set; }
    public string ValuesJson { get; set; } = "{}";
    public string? ApplicantName { get; set; }
    public string? ApplicantEmail { get; set; }
    public string? UserAgent { get; set; }
    public string? PdfHash { get; set; }
    public bool ConsentGiven { get; set; }
    public string? Notes { get; set; }

    public Dictionary<string, string> Values
    {
        get
        {
            try { return JsonSerializer.Deserialize<Dictionary<string, string>>(ValuesJson) ?? []; }
            catch { return []; }
        }
    }
}
