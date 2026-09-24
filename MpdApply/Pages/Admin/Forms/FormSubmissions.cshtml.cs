using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MpdApply.Data;
using MpdApply.Models;
using MpdApply.Services;

namespace MpdApply.Pages.Admin.Forms;

public class FormSubmissionsModel(AppDbContext db, ILogger<FormSubmissionsModel> logger) : PageModel
{
    public FormTemplate Template { get; private set; } = null!;
    public List<FormSubmission> Submissions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;
        var t = await db.FormTemplates.FindAsync(id);
        if (t == null) return NotFound();
        Template = t;
        Submissions = await db.FormSubmissions
            .Where(s => s.FormTemplateId == id)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetDownloadAsync(Guid id, Guid sid)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        var template = await db.FormTemplates.FindAsync(id);
        var submission = await db.FormSubmissions.FindAsync(sid);
        if (template == null || submission == null) return NotFound();

        try
        {
            var logoSetting = await db.Settings.FindAsync("LogoBase64");
            var agencySetting = await db.Settings.FindAsync("AgencyName");
            var agency = agencySetting?.Value ?? "Town of Middletown Police Department";
            var pdf = GenericPdfGenerator.Generate(template, submission, logoSetting?.Value, agency);
            var name = submission.ApplicantName?.Replace(" ", "_") ?? submission.Id.ToString()[..8];
            return File(pdf, "application/pdf", $"{template.Slug}_{name}.pdf");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PDF generation failed for submission {Id}", sid);
            return Content($"PDF error: {ex.GetType().Name}: {ex.Message}", "text/plain");
        }
    }

    public async Task<IActionResult> OnGetPreviewAsync(Guid id, Guid sid)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        var template = await db.FormTemplates.FindAsync(id);
        var submission = await db.FormSubmissions.FindAsync(sid);
        if (template == null || submission == null) return NotFound();

        try
        {
            var logoSetting = await db.Settings.FindAsync("LogoBase64");
            var agencySetting = await db.Settings.FindAsync("AgencyName");
            var agency = agencySetting?.Value ?? "Town of Middletown Police Department";
            var pdf = GenericPdfGenerator.Generate(template, submission, logoSetting?.Value, agency);
            return File(pdf, "application/pdf");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PDF preview failed for submission {Id}", sid);
            return Content($"PDF error: {ex.Message}", "text/plain");
        }
    }

    public async Task<IActionResult> OnGetExportAsync(Guid id)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        var template = await db.FormTemplates.FindAsync(id);
        if (template == null) return NotFound();

        var submissions = await db.FormSubmissions
            .Where(s => s.FormTemplateId == id)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        var fields = template.Schema.Pages
            .SelectMany(p => p.Fields)
            .Where(f => f.Type is not ("section_header" or "paragraph" or "initials_panel" or "initials_reminder"))
            .ToList();

        var sb = new System.Text.StringBuilder();
        var headers = new List<string> { "Name", "Email", "Submitted", "Status", "Notes", "IP" };
        headers.AddRange(fields.Select(f => f.Label ?? f.Key));
        sb.AppendLine(string.Join(",", headers.Select(CsvEsc)));

        foreach (var s in submissions)
        {
            var vals = s.Values;
            var row = new List<string>
            {
                s.ApplicantName ?? "", s.ApplicantEmail ?? "",
                s.SubmittedAt?.ToLocalTime().ToString("M/d/yyyy h:mm tt") ?? "",
                s.Status, s.Notes ?? "", s.IpAddress ?? ""
            };
            foreach (var f in fields)
            {
                vals.TryGetValue(f.Key, out var v);
                row.Add(f.Type == "signature" && !string.IsNullOrWhiteSpace(v) && v.StartsWith("data:") ? "[signature]" : v ?? "");
            }
            sb.AppendLine(string.Join(",", row.Select(CsvEsc)));
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"{template.Slug}-submissions.csv");
    }

    public async Task<IActionResult> OnPostStatusAsync(Guid id, Guid sid, string status, string? notes)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;
        var s = await db.FormSubmissions.FindAsync(sid);
        if (s != null) { s.Status = status; s.Notes = notes; await db.SaveChangesAsync(); }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteSubmissionAsync(Guid id, Guid sid)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;
        var s = await db.FormSubmissions.FindAsync(sid);
        if (s != null) { db.FormSubmissions.Remove(s); await db.SaveChangesAsync(); }
        return RedirectToPage(new { id });
    }

    static string CsvEsc(string? v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        if (v.Contains(',') || v.Contains('"') || v.Contains('\n') || v.Contains('\r'))
            return '"' + v.Replace("\"", "\"\"") + '"';
        return v;
    }
}
