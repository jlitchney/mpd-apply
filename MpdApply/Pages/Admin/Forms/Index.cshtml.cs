using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MpdApply.Data;
using MpdApply.Models;
using MpdApply.Services;
using System.Text.Json;

namespace MpdApply.Pages.Admin.Forms;

public class FormsIndexModel(AppDbContext db) : PageModel
{
    public List<(FormTemplate Template, int SubmissionCount)> Templates { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        var templates = await db.FormTemplates.OrderByDescending(t => t.CreatedAt).ToListAsync();
        Templates = templates.Select(t => (t, db.FormSubmissions.Count(s => s.FormTemplateId == t.Id))).ToList();

        return Page();
    }

    public async Task<IActionResult> OnGetSamplePdfAsync(Guid id)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        var template = await db.FormTemplates.FindAsync(id);
        if (template == null) return NotFound();

        var values = new Dictionary<string, string>();
        foreach (var page in template.Schema.Pages)
        {
            foreach (var field in page.Fields)
            {
                values[field.Key] = field.Type switch
                {
                    "signature" or "initials_panel" or "initials_reminder"
                        or "section_header" or "paragraph" => "",
                    "initials" => "JS",
                    "date" => DateTime.Today.ToString("MM/dd/yyyy"),
                    "yesno" => "Yes",
                    "radio" => field.Options?.FirstOrDefault() ?? "",
                    "select" => field.Options?.FirstOrDefault() ?? "",
                    "checkbox" => "true",
                    "textarea" => "(Sample text)",
                    _ => "(Sample)"
                };
            }
        }
        values["InitialsMode"] = "typed";

        var logoSetting = await db.Settings.FindAsync("LogoBase64");
        var agencySetting = await db.Settings.FindAsync("AgencyName");
        var agency = agencySetting?.Value ?? "Town of Middletown Police Department";

        var submission = new FormSubmission
        {
            FormTemplateId = template.Id,
            Status = "Sample",
            SubmittedAt = DateTime.UtcNow,
            IpAddress = "0.0.0.0",
            ValuesJson = JsonSerializer.Serialize(values),
            ApplicantName = "Sample Applicant"
        };

        var pdf = GenericPdfGenerator.Generate(template, submission, logoSetting?.Value, agency);
        return File(pdf, "application/pdf", $"{template.Slug}-sample.pdf");
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid id)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;
        var t = await db.FormTemplates.FindAsync(id);
        if (t != null) { t.IsPublished = !t.IsPublished; await db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;
        var t = await db.FormTemplates.FindAsync(id);
        if (t != null)
        {
            db.FormSubmissions.RemoveRange(db.FormSubmissions.Where(s => s.FormTemplateId == id));
            db.FormTemplates.Remove(t);
            await db.SaveChangesAsync();
            TempData["Message"] = $"Deleted form \"{t.Name}\".";
        }
        return RedirectToPage();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Remove("AdminAuth");
        return RedirectToPage("/Admin/Login");
    }
}
