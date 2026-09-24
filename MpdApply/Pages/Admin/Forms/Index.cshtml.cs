using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MpdApply.Data;
using MpdApply.Models;

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
