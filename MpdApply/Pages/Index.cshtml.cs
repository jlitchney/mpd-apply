using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MpdApply.Data;
using MpdApply.Models;

namespace MpdApply.Pages;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<FormTemplate> PublishedForms { get; private set; } = [];
    public string? LogoBase64 { get; private set; }

    public async Task OnGetAsync()
    {
        PublishedForms = await db.FormTemplates
            .Where(t => t.IsPublished)
            .OrderBy(t => t.Name)
            .ToListAsync();
        LogoBase64 = (await db.Settings.FirstOrDefaultAsync(s => s.Key == "LogoBase64"))?.Value;
    }
}
