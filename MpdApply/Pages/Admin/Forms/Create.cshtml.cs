using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;
using MpdApply.Models;
using MpdApply.Services;

namespace MpdApply.Pages.Admin.Forms;

public class CreateModel(AppDbContext db, ClaudeService claude, IConfiguration config) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    public bool ClaudeConfigured => !string.IsNullOrWhiteSpace(config["Claude:ApiKey"]);

    public class InputModel
    {
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Description { get; set; }
        public string? RecipientEmail { get; set; }
        public bool IsPublished { get; set; } = true;
        public string SchemaJson { get; set; } = """
            {
              "pages": [
                {
                  "title": "Page 1",
                  "intro": "",
                  "fields": [
                    {"key":"name","type":"text","label":"Full Name","required":true,"width":"full"},
                    {"key":"email","type":"email","label":"Email","required":true,"width":"half"},
                    {"key":"signature","type":"signature","label":"Signature","required":true,"width":"full"}
                  ]
                }
              ]
            }
            """;
    }

    public IActionResult OnGet()
    {
        var auth = LoginModel.RequireAuth(this);
        return auth ?? Page();
    }

    public async Task<IActionResult> OnPostFromPdfAsync(string pdfName, IFormFile? pdfFile)
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        if (pdfFile == null || pdfFile.Length == 0)
        {
            TempData["Error"] = "Please select a PDF file.";
            return Page();
        }

        // Extract text using Python pdfminer (available on Railway)
        string pdfText;
        try
        {
            var tempPath = Path.GetTempFileName() + ".pdf";
            await using (var fs = System.IO.File.Create(tempPath))
                await pdfFile.CopyToAsync(fs);

            var result = await RunProcess("python3", $"-c \"from pdfminer.high_level import extract_text; print(extract_text('{tempPath}'))\"");
            pdfText = result.Trim();
            System.IO.File.Delete(tempPath);

            if (string.IsNullOrWhiteSpace(pdfText))
            {
                TempData["Error"] = "Could not extract text from the PDF.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"PDF extraction failed: {ex.Message}";
            return Page();
        }

        // Call Claude
        var schema = await claude.ConvertPdfTextToSchema(pdfText);
        if (schema == null)
        {
            TempData["Error"] = "AI conversion failed. Please check that Claude:ApiKey is configured.";
            return Page();
        }

        // Pre-populate the manual form for review
        Input.Name = pdfName;
        Input.Slug = pdfName.ToLower().Replace(" ", "-").Replace("'", "");
        Input.SchemaJson = schema;
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var auth = LoginModel.RequireAuth(this);
        if (auth != null) return auth;

        // Validate slug uniqueness
        if (db.FormTemplates.Any(t => t.Slug == Input.Slug))
        {
            TempData["Error"] = $"Slug \"{Input.Slug}\" is already in use. Choose a different one.";
            return Page();
        }

        // Validate schema parses
        var schema = Models.FormSchemaDef.Parse(Input.SchemaJson);
        if (!schema.Pages.Any())
        {
            TempData["Error"] = "Schema is invalid or has no pages. Check the JSON.";
            return Page();
        }

        db.FormTemplates.Add(new FormTemplate
        {
            Name = Input.Name,
            Slug = Input.Slug,
            Description = Input.Description,
            RecipientEmail = Input.RecipientEmail,
            IsPublished = Input.IsPublished,
            SchemaJson = Input.SchemaJson
        });
        await db.SaveChangesAsync();

        TempData["Message"] = $"Form \"{Input.Name}\" created. URL: /Forms/{Input.Slug}";
        return RedirectToPage("/Admin/Forms/Index");
    }

    private static async Task<string> RunProcess(string cmd, string args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo(cmd, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var proc = System.Diagnostics.Process.Start(psi)!;
        var output = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();
        return output;
    }

    public const string SchemaHelpText = """
        Field types: text, textarea, date, email, tel, select, radio, yesno, checkbox,
                     signature, initials, initials_panel, initials_reminder, section_header, paragraph

        Width values: full, half, third, quarter

        Example field definitions:
          {"key":"name","type":"text","label":"Full Name","required":true,"width":"full"}
          {"key":"dob","type":"date","label":"Date of Birth","width":"half"}
          {"key":"notes","type":"textarea","label":"Notes","rows":4,"width":"full"}
          {"key":"gender","type":"radio","label":"Gender","options":["Male","Female"],"width":"half"}
          {"key":"agree","type":"yesno","label":"Do you agree?","width":"half"}
          {"key":"role","type":"select","label":"Role","options":["Officer","Admin"],"width":"half"}
          {"key":"sig","type":"signature","label":"Signature","required":true,"width":"full"}
          {"key":"init_item","type":"initials","label":"I acknowledge this item","width":"full"}
          {"key":"initials_setup","type":"initials_panel"}   ← place before initials fields
          {"key":"note","type":"paragraph","content":"Please read carefully..."}
          {"key":"sec","type":"section_header","label":"Section Title"}
        """;
}
