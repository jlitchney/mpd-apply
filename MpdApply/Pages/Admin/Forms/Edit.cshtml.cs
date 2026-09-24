using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MpdApply.Data;
using MpdApply.Models;
using MpdApply.Services;
using System.Text.Json;

namespace MpdApply.Pages.Admin.Forms;

public class EditModel(AppDbContext db) : PageModel
{
    public FormTemplate Template { get; private set; } = null!;
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Description { get; set; }
        public string? RecipientEmail { get; set; }
        public bool IsPublished { get; set; } = true;
        public string SchemaJson { get; set; } = "";
    }

    [BindProperty] public string? PreviewSchemaJson { get; set; }
    [BindProperty] public string? PreviewName       { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;
        var t = await db.FormTemplates.FindAsync(id);
        if (t == null) return NotFound();
        Template = t;
        Input = new InputModel
        {
            Name = t.Name, Slug = t.Slug, Description = t.Description,
            RecipientEmail = t.RecipientEmail, IsPublished = t.IsPublished,
            SchemaJson = t.SchemaJson
        };
        return Page();
    }

    public async Task<IActionResult> OnPostPreviewPdfAsync(Guid id)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        var template = await db.FormTemplates.FindAsync(id);
        if (template == null) return NotFound();

        var schemaJson = !string.IsNullOrWhiteSpace(PreviewSchemaJson) ? PreviewSchemaJson : template.SchemaJson;
        var name       = !string.IsNullOrWhiteSpace(PreviewName)       ? PreviewName       : template.Name;

        var preview = new FormTemplate { Id = template.Id, Name = name, Slug = template.Slug, SchemaJson = schemaJson };

        var values = new Dictionary<string, string>();
        foreach (var page in preview.Schema.Pages)
            foreach (var field in page.Fields)
                values[field.Key] = field.Type switch
                {
                    "signature" or "initials_panel" or "initials_reminder"
                        or "section_header" or "paragraph" => "",
                    "initials"  => "JS",
                    "date"      => DateTime.Today.ToString("MM/dd/yyyy"),
                    "yesno"     => "Yes",
                    "radio"     => field.Options?.FirstOrDefault() ?? "",
                    "select"    => field.Options?.FirstOrDefault() ?? "",
                    "checkbox"  => "true",
                    "textarea"  => "(Sample text)",
                    _           => "(Sample)"
                };
        values["InitialsMode"] = "typed";

        var logoSetting   = await db.Settings.FindAsync("LogoBase64");
        var agencySetting = await db.Settings.FindAsync("AgencyName");
        var agency        = agencySetting?.Value ?? "Town of Middletown Police Department";

        var submission = new FormSubmission
        {
            FormTemplateId = template.Id, Status = "Sample",
            SubmittedAt = DateTime.UtcNow, IpAddress = "0.0.0.0",
            ValuesJson = JsonSerializer.Serialize(values), ApplicantName = "Sample Applicant"
        };

        var pdf = GenericPdfGenerator.Generate(preview, submission, logoSetting?.Value, agency);
        return File(pdf, "application/pdf", $"{template.Slug}-style-preview.pdf");
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;
        var t = await db.FormTemplates.FindAsync(id);
        if (t == null) return NotFound();
        Template = t;

        var schema = FormSchemaDef.Parse(Input.SchemaJson);
        if (!schema.Pages.Any())
        {
            TempData["Error"] = "Schema is invalid or has no pages.";
            return Page();
        }

        t.Name = Input.Name;
        t.Slug = Input.Slug;
        t.Description = Input.Description;
        t.RecipientEmail = Input.RecipientEmail;
        t.IsPublished = Input.IsPublished;
        t.SchemaJson = Input.SchemaJson;
        await db.SaveChangesAsync();

        TempData["Message"] = "Changes saved.";
        return RedirectToPage(new { id });
    }
}
