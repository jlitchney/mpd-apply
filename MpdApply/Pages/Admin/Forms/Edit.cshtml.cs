using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MpdApply.Data;
using MpdApply.Models;

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
