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

    public async Task<IActionResult> OnGetViewAsync(Guid id, Guid sid)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        var template = await db.FormTemplates.FindAsync(id);
        var submission = await db.FormSubmissions.FindAsync(sid);
        if (template == null || submission == null) return NotFound();

        var values = submission.Values;
        var lines = new System.Text.StringBuilder();
        lines.AppendLine($"Form: {template.Name}");
        lines.AppendLine($"Submitted: {submission.SubmittedAt?.ToLocalTime():M/d/yyyy h:mm tt}");
        lines.AppendLine($"IP: {submission.IpAddress}");
        lines.AppendLine("---");
        foreach (var kv in values)
        {
            if (kv.Key.StartsWith("InitialsImageData") || (kv.Value.Length > 200 && kv.Value.StartsWith("data:")))
                lines.AppendLine($"{kv.Key}: [image data]");
            else
                lines.AppendLine($"{kv.Key}: {kv.Value}");
        }
        return Content(lines.ToString(), "text/plain");
    }
}
