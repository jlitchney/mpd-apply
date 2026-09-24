using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;
using MpdApply.Models;
using MpdApply.Services;
using System.Security.Cryptography;
using System.Text.Json;

namespace MpdApply.Pages.Forms;

public class ViewModel(AppDbContext db, EmailService email, ILogger<ViewModel> logger) : PageModel
{
    public FormTemplate Template { get; private set; } = null!;
    public FormPageDef PageDef { get; private set; } = null!;
    public int CurrentPage { get; private set; } = 1;
    public int TotalPages { get; private set; }
    public Dictionary<string, string> SessionValues { get; private set; } = [];
    public string? LogoBase64 { get; private set; }
    public string? AgencyName { get; private set; }
    public bool IsAdminPreview => HttpContext.Session.GetString("AdminAuth") == "true";

    private string SessionKey => $"form_{Template.Slug}";

    public async Task<IActionResult> OnGetAsync(string slug, int? step)
    {
        var template = await db.FormTemplates.FindAsync(
            db.FormTemplates.Where(t => t.Slug == slug).Select(t => t.Id).FirstOrDefault());

        if (template == null || !template.IsPublished) return NotFound();
        Template = template;
        TotalPages = Template.Schema.Pages.Count;
        CurrentPage = Math.Clamp(step ?? 1, 1, TotalPages);
        PageDef = Template.Schema.Pages[CurrentPage - 1];
        SessionValues = LoadSession();
        await LoadBrandingAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug, int? step)
    {
        var template = db.FormTemplates
            .FirstOrDefault(t => t.Slug == slug);
        if (template == null || !template.IsPublished) return NotFound();
        Template = template;
        TotalPages = Template.Schema.Pages.Count;
        CurrentPage = Math.Clamp(step ?? 1, 1, TotalPages);
        PageDef = Template.Schema.Pages[CurrentPage - 1];

        // Collect this page's values
        var pageValues = new Dictionary<string, string>();
        foreach (var field in PageDef.Fields)
        {
            if (field.Type is "section_header" or "paragraph" or "initials_panel" or "initials_reminder")
                continue;

            var raw = Request.Form[field.Key].FirstOrDefault() ?? "";
            pageValues[field.Key] = raw;
        }

        // Always capture global initials fields if present
        foreach (var key in new[] { "InitialsMode", "InitialsImageData" })
        {
            var v = Request.Form[key].FirstOrDefault();
            if (v != null) pageValues[key] = v;
        }

        // Capture signature timestamps
        foreach (var field in PageDef.Fields.Where(f => f.Type == "signature"))
        {
            var v = Request.Form[$"{field.Key}__signedAt"].FirstOrDefault();
            if (v != null) pageValues[$"{field.Key}__signedAt"] = v;
        }

        // Validate required fields on current page
        var errors = new List<string>();
        foreach (var field in PageDef.Fields)
        {
            if (!field.Required) continue;
            if (field.Type is "section_header" or "paragraph" or "initials_panel" or "initials_reminder") continue;
            pageValues.TryGetValue(field.Key, out var val);
            if (string.IsNullOrWhiteSpace(val) || val == "data:,")
                errors.Add(field.Label ?? field.Key);
        }
        if (!IsAdminPreview)
        {
            if (errors.Any())
            {
                TempData["Error"] = $"Please complete all required fields: {string.Join(", ", errors)}.";
                SessionValues = MergeSession(pageValues);
                await LoadBrandingAsync();
                return Page();
            }

            if (CurrentPage == TotalPages && Request.Form["EsignConsent"].FirstOrDefault() != "true")
            {
                TempData["Error"] = "Please read and check the electronic signature consent box before submitting.";
                SessionValues = MergeSession(pageValues);
                await LoadBrandingAsync();
                return Page();
            }
        }

        // Save to session
        var merged = MergeSession(pageValues);
        SaveSession(merged);

        // Not the last page — advance
        if (CurrentPage < TotalPages)
            return Redirect($"/Forms/{slug}/{CurrentPage + 1}");

        // Final page — create submission
        var submission = new FormSubmission
        {
            FormTemplateId = template.Id,
            Status = "Submitted",
            SubmittedAt = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            ConsentGiven = true,
            ValuesJson = JsonSerializer.Serialize(merged),
            ApplicantName = merged.GetValueOrDefault("name")
                ?? merged.GetValueOrDefault("full_name")
                ?? (merged.TryGetValue("first_name", out var fn) && merged.TryGetValue("last_name", out var ln)
                    ? $"{fn} {ln}".Trim() : null),
            ApplicantEmail = merged.GetValueOrDefault("email")
        };

        db.FormSubmissions.Add(submission);
        await db.SaveChangesAsync();

        // Generate PDF, hash it, and email
        try
        {
            var logoSetting = await db.Settings.FindAsync("LogoBase64");
            var agencySetting = await db.Settings.FindAsync("AgencyName");
            var recipient = template.RecipientEmail
                ?? (await db.Settings.FindAsync("RecipientEmail"))?.Value
                ?? "jason@allstartalent.us";
            var agency = agencySetting?.Value ?? "Town of Middletown Police Department";

            var pdf = GenericPdfGenerator.Generate(template, submission, logoSetting?.Value, agency);

            submission.PdfHash = Convert.ToHexString(SHA256.HashData(pdf));
            await db.SaveChangesAsync();

            await email.SendApplicationAsync(recipient, submission.ApplicantName ?? "Applicant", pdf,
                $"{template.Name} — {submission.ApplicantName}");

            if (!string.IsNullOrWhiteSpace(submission.ApplicantEmail))
                await email.SendConfirmationAsync(submission.ApplicantEmail, submission.ApplicantName ?? "Applicant", template.Name, pdf);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PDF/email failed for submission {Id}", submission.Id);
        }

        // Clear session
        HttpContext.Session.Remove(SessionKey);
        return RedirectToPage("/Forms/Confirmation", new { name = submission.ApplicantName ?? "Applicant", form = template.Name });
    }

    private Dictionary<string, string> LoadSession()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return [];
        try { return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? []; }
        catch { return []; }
    }

    private Dictionary<string, string> MergeSession(Dictionary<string, string> newValues)
    {
        var existing = LoadSession();
        foreach (var kv in newValues) existing[kv.Key] = kv.Value;
        return existing;
    }

    private void SaveSession(Dictionary<string, string> values)
    {
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(values));
    }

    private async Task LoadBrandingAsync()
    {
        LogoBase64 = (await db.Settings.FindAsync("LogoBase64"))?.Value;
        AgencyName = (await db.Settings.FindAsync("AgencyName"))?.Value;
    }
}
